using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Beast.Claw.Gizmo
{
    public class GizmoRender : Controller, IGizmoManager, IGameCycleListener
    {
        private struct HandleInfo
        {
            public GizmoHandle handle;
            public IController owner;
        }

        private const int MAX_TEXTURE_SIZE = 128;
        
        private readonly ICameraTransform _camera;
        private readonly IScreen _screen;
        private readonly GizmoConfig _config;

        private RenderTexture _canvas;
        private int2 _canvasSize;
        private List<HandleInfo> _handles;
        private Material _canvasMaterial;
        private Material _shapeMaterial;

        private Texture2D[] _texturePool;
        private NativeArray<ushort> _floatBuffer;

        private CommandBuffer _commandBuffer;

        private int _prevTexturePower;
        private int _prevShapeCount;

        public GizmoRender(
            ICameraTransform camera, 
            IScreen screen,
            GizmoConfig config)
        {
            _camera = camera;
            _screen = screen;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        public IGizmoHandle CreateHandle(IController owner)
        {
            owner.ValidateNotNullThrowException(nameof(owner));

            HandleInfo info = new ()
            {
                handle = new GizmoHandle(),
                owner = owner,
            };

            _handles.Add(info);

            return info.handle;
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            Texture2DParameters textureData = new()
            {
                createInitialized = false,
                format = TextureFormat.RGBAHalf,
                isLinear = true,
                mipChain = false,
                texture = new PlanarTextureData
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                }
            };
            
            int maxTexturePower = BMath.CeilToInt(BMath.Log2(MAX_TEXTURE_SIZE));
            _texturePool = new Texture2D[maxTexturePower + 1];
            
            for (int i = 0; i <= maxTexturePower; i++)
            {
                _texturePool[i] = BGraphic.CreateTexture2D(this, textureData.WithSize(1 << i));
            }
            
            _handles = new List<HandleInfo>(64);
            
            _canvasMaterial = BGraphic.CreateMaterial(this, _config.canvasShader);
            _shapeMaterial = BGraphic.CreateMaterial(this, _config.shapeShader);
            _commandBuffer = new CommandBuffer();

            _canvas = _setupCanvas(_screen.Size);
            
            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_canvasSize.Equals(_screen.Size))
            {
                BGraphic.Dispose(_canvas);

                _canvas = _setupCanvas(_screen.Size);
            }

            int shapeCount = 0;
            
            Profiler.BeginSample("CreateFloatBuffer");
            for (int i = 0; i < _handles.Count; i++)
            {
                HandleInfo handle = _handles[i];

                if (handle.handle.IsDisposed)
                {
                    _handles.RemoveAtSwapBack(i--);
                    continue;
                }

                int handleShapeCount = handle.handle.shapeCount;
                if (handleShapeCount == 0)
                {
                    continue;
                }
                
                GizmoUtil.EnsureCapacity(ref _floatBuffer, shapeCount + handleShapeCount);
                
                _floatBuffer.Slice(shapeCount * GizmoUtil.FLOAT_PER_SHAPE, handleShapeCount * GizmoUtil.FLOAT_PER_SHAPE)
                    .CopyFrom(handle.handle.buffer.Slice(0, handleShapeCount * GizmoUtil.FLOAT_PER_SHAPE));

                shapeCount += handleShapeCount;
            }
            Profiler.EndSample();

            if (shapeCount == 0)
            {
                return;
            }

            int textureSize = BMath.CeilToInt(BMath.Sqrt(_floatBuffer.Length / 4.0f));
            if (textureSize > MAX_TEXTURE_SIZE)
            {
                BDebug.LogError(this, $"Too big gizmo buffer: ({textureSize * textureSize})");
                return;
            }

            int texturePower2 = BMath.CeilToInt(BMath.Log2(textureSize));
            Texture2D texture = _texturePool[texturePower2];
                
            Profiler.BeginSample("SetPixelData");
            texture.SetPixelData(_floatBuffer, 0);
            texture.Apply();
            Profiler.EndSample();

            if (Build.IS_EDITOR || _prevTexturePower != texturePower2)
            {
                Profiler.BeginSample("SetShapeMaterialParams #2");
                _shapeMaterial.SetTexture("_DataTexture", texture);
                _shapeMaterial.SetFloat("_DataTextureSize", 1 << texturePower2);
                _prevTexturePower = texturePower2;
                Profiler.EndSample();
            }

            if (Build.IS_EDITOR)
            {
                _setupMaterialsRare();
            }
            
            Profiler.BeginSample("SetShapeMaterialParams #1");
            _shapeMaterial.SetFloat2("_RenderCenter", _camera.Position);
            _shapeMaterial.SetFloat("_RenderDistance", _camera.ViewDistance);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetRenderTarget");
            Graphics.SetRenderTarget(_canvas);
            Profiler.EndSample();
            
            GL.Clear(true, true, Color.clear);

            if (_prevShapeCount != shapeCount)
            {
                Profiler.BeginSample("SetCommandBufferParams");
                _commandBuffer.Clear();
                _commandBuffer.SetRenderTarget(_canvas);
                _commandBuffer.DrawMeshInstancedProcedural(_config.quadMesh, 0, _shapeMaterial, 0, shapeCount);
                Profiler.EndSample();
                _prevShapeCount = shapeCount;
            }

            Profiler.BeginSample("ExecuteCommandBuffer");
            BGraphic.ExecuteBuffer(_commandBuffer);
            Profiler.EndSample();

            Graphics.SetRenderTarget(null);
            
            Profiler.BeginSample("DrawCanvas");
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _canvasMaterial,
                matrix = new Float4x4TRS
                {
                    rotation = Quaternion.identity,
                    scale = new float3(_screen.Aspect, 1, 1),
                    translation = new float3(0, 0, RenderOrderZ.Gizmo)
                },
                mesh = _config.quadMesh
            });
            Profiler.EndSample();
        }

        private RenderTexture _setupCanvas(int2 screenSize)
        {
            RenderTexture canvas = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
            {
                format = RenderTextureFormat.ARGB32,
                texture = new PlanarTextureData
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    size = screenSize
                },
                temp = new TempRenderTextureData
                {
                    antiAliasing = 1
                }
            });
            
            _canvasSize = screenSize;
            
            _setupMaterialsRare();

            return canvas;
        }

        private void _setupMaterialsRare()
        {
            _shapeMaterial.SetFloat2("_ScreenSize", _canvasSize);
            _canvasMaterial.SetTexture("_Texture", _canvas);

            float rollCos = BMath.Cos(_camera.RotationRad.roll);
            float pitchCos = BMath.Cos(_camera.RotationRad.pitch);
            
            float2x2 rotation = float2x2.Rotate(_camera.RotationRad.roll * -1);
            float2x2 scale = float2x2.Scale(1, pitchCos);
            
            _shapeMaterial.SetFloat2x2("_ViewMatrix0", rotation);
            _shapeMaterial.SetFloat2x2("_ViewMatrix1", scale);
            
            _shapeMaterial.SetFloat("_ProjectionScale", 1f / (rollCos * pitchCos));
        }
        
        private void _dispose(BlockingContext context)
        {
            BGraphic.Dispose(_canvas);

            foreach (Texture2D texture in _texturePool)
            {
                BGraphic.Dispose(texture);
            }

            _floatBuffer.Dispose();
            _commandBuffer.Dispose();

            BGraphic.Dispose(_canvasMaterial);
            BGraphic.Dispose(_shapeMaterial);
        }
    }
}