using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Beast.Claw.Gizmos
{
    public class GizmosRender : Controller, IGizmosManager, IGameCycleListener
    {
        public int RenderVersion { get; private set; }
        
        private const int BUFFER_SIZE = 256;
        private const int TEXTURE_SIZE = 32;
        
        private readonly ICameraTransform _camera;
        private readonly List<IGizmosDrawer> _drawControllers;
        private readonly IScreen _screen;
        private readonly GizmosConfig _config;

        private RenderTexture _canvasTexture;
        private List<GizmosHandle> _handles;
        private Material _blitMaterial;
        private Material _quadRenderMaterial;

        private GizmosBuffer<GizmosQuad> _quads;
        
        private Texture2D _rgba32Texture;
        private NativeArray<byte> _rgba32Buffer;

        private Texture2D _rgFloatTexture;
        private NativeArray<float> _rgFloatBuffer;

        public GizmosRender(
            ICameraTransform camera, 
            List<IGizmosDrawer> drawers,
            IScreen screen,
            GizmosConfig config)
        {
            _camera = camera;
            _drawControllers = drawers;
            _screen = screen;
            _config = config;

            GizmosStateStorage.Initialize();
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _handles = new List<GizmosHandle>(_drawControllers.Count);
            
            foreach (IGizmosDrawer drawer in _drawControllers)
            {
                _handles.Add(GetHandle(drawer, drawer.GizmosColor));
            }

            _blitMaterial = BGraphic.CreateMaterial(this, _config.blitShader);
            _quadRenderMaterial = BGraphic.CreateMaterial(this, _config.quadShader);

            _quads = new GizmosBuffer<GizmosQuad>(BUFFER_SIZE);

            _rgba32Texture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                isLinear = true,
                createInitialized = false,
                format = TextureFormat.RGBA32,
                mipChain = false,
                texture = new PlanarTextureData
                {
                    size = TEXTURE_SIZE,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                }
            });
            _rgba32Buffer = Native.CreatePermArray<byte>(TEXTURE_SIZE * TEXTURE_SIZE * 4);

            _rgFloatTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                isLinear = true,
                createInitialized = false,
                format = TextureFormat.RGFloat,
                mipChain = false,
                texture = new PlanarTextureData
                {
                    size = TEXTURE_SIZE,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                }
            });
            _rgFloatBuffer = Native.CreatePermArray<float>(TEXTURE_SIZE * TEXTURE_SIZE * 2);

            return UniTask.CompletedTask;
        }

        public GizmosHandle GetHandle(IController owner, string handleName, Color defaultColor)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            handleName.ValidateNotEmptyOrNullThrowException(nameof(handleName));
            
            handleName = owner.Meta.name + "_" + handleName;
            
            ushort id = GizmosStateStorage.GetOrCreateHandleId(handleName, defaultColor);
            
            return new GizmosHandle(id, this);
        }

        public GizmosHandle GetHandle(IController owner, Color defaultColor)
        {
            ushort id = GizmosStateStorage.GetOrCreateHandleId(owner.Meta.name, defaultColor);
            
            return new GizmosHandle(id, this);
        }

        private void _render(BlockingContext context)
        {
            if (_canvasTexture == null || 
                _canvasTexture.width != _screen.Width || 
                _canvasTexture.height != _screen.Height)
            {
                BGraphic.TryDispose(ref _canvasTexture);
            
                _canvasTexture = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
                {
                    format = RenderTextureFormat.ARGB32,
                    texture = new PlanarTextureData
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        size = _screen.Size
                    },
                    temp = new TempRenderTextureData
                    {
                        antiAliasing = 1
                    }
                });
            }
            
            Profiler.BeginSample("InvokeDrawers");
            for (int i = 0; i < _drawControllers.Count; i++)
            {
                IGizmosDrawer drawer = _drawControllers[i];
                
                Profiler.BeginSample(drawer.Meta.name);
                drawer.Draw(_handles[i]);
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Graphics.SetRenderTarget(_canvasTexture);
            GL.Clear(true, true, Color.clear);

            if (_quads.shapeCount > 0)
            {
                float rollRad = _camera.RotationRad.roll;
                quaternion rotation = new QuaternionEulerRad(BMath.PI * 0.5f - _camera.RotationRad.pitch, rollRad, 0);
                float4x4 viewMatrix = new Float4x4TRS
                {
                    translation = _camera.Position.X0Y() + rotation.Mul(new float3(0, 0, _camera.ViewDistance)),
                    scale = 1,
                    rotation = rotation
                };

                QuadToTextureJob job = new()
                {
                    rgFloatTexture = _rgFloatBuffer,
                    rgba32Texture = _rgba32Buffer,
                    quads = _quads.shapes
                };

                Profiler.BeginSample("QuadToTextureJob");
                job.Schedule(_quads.shapeCount, 128).Complete();
                Profiler.EndSample();
                
                Profiler.BeginSample("ApplyPixelData");
                _rgba32Texture.SetPixelData(_rgba32Buffer, 0);
                _rgFloatTexture.SetPixelData(_rgFloatBuffer, 0);
                _rgba32Texture.Apply();
                _rgFloatTexture.Apply();
                Profiler.EndSample();

                _quadRenderMaterial.SetTexture("_RGBA32Texture", _rgba32Texture);
                _quadRenderMaterial.SetTexture("_RGFloatTexture", _rgFloatTexture);
                _quadRenderMaterial.SetFloat("_TextureSize", TEXTURE_SIZE);
            
                viewMatrix = math.inverse(viewMatrix);

                float rollSin = BMath.Sin(rollRad);
                float4x4 projectionMatrix = float4x4.Ortho(
                    _camera.ViewDistance * _screen.Aspect * rollSin,
                    _camera.ViewDistance * rollSin, 
                    0.01f, 
                    10000);
                
                using CommandBuffer commandBuffer = BGraphic.GetTempBuffer(this);
                commandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                commandBuffer.SetRenderTarget(_canvasTexture);
                commandBuffer.DrawMeshInstancedProcedural(_config.quad, 0, _quadRenderMaterial, 0, _quads.shapeCount);

                BGraphic.ExecuteBuffer(commandBuffer);
            }

            RenderVersion++;

            _blitMaterial.SetTexture("_Texture", _canvasTexture);

            Graphics.SetRenderTarget(null);
            
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _blitMaterial,
                matrix = new Float4x4TRS
                {
                    rotation = Quaternion.identity,
                    scale = new float3(_screen.Aspect, 1, 1),
                    translation = new float3(0, 0, RenderOrderZ.Gizmos)
                },
                mesh = _config.quad
            });
            
            _quads.Clear();
        }

        public void ScheduleQuad(GizmosQuad quad)
        {
            _quads.Add(quad);
        }
        
        private void _dispose(BlockingContext context)
        {
            _quads.Dispose();
            
            GizmosStateStorage.Dispose();

            BGraphic.TryDispose(ref _canvasTexture);
            BGraphic.TryDispose(ref _blitMaterial);
            BGraphic.TryDispose(ref _rgba32Texture);
            BGraphic.TryDispose(ref _rgFloatTexture);
            _rgba32Buffer.Dispose();
            _rgFloatBuffer.Dispose();
        }
    }
}