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
        private NativeArray<float> _floatBuffer;

        private CommandBuffer _commandBuffer;

        private int _prevTexturePower;

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
                format = TextureFormat.RGBAFloat,
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

            _canvas = _createCanvas(_screen.Size);
            _canvasSize = _screen.Size;
            _shapeMaterial.SetFloat2("_ScreenSize", _screen.Size);
            _canvasMaterial.SetTexture("_Texture", _canvas);

            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_canvasSize.Equals(_screen.Size))
            {
                BGraphic.Dispose(_canvas);

                _canvas = _createCanvas(_screen.Size);
                _canvasSize = _screen.Size;
                _shapeMaterial.SetFloat2("_ScreenSize", _screen.Size);
                _canvasMaterial.SetTexture("_Texture", _canvas);
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

            if (_prevTexturePower != texturePower2)
            {
                Profiler.BeginSample("SetShapeMaterialParams");
                _shapeMaterial.SetTexture("_DataTexture", texture);
                _shapeMaterial.SetFloat("_DataTextureSize", 1 << texturePower2);
                _prevTexturePower = texturePower2;
                Profiler.EndSample();
            }
            
            Profiler.BeginSample("SetRenderTarget");
            Graphics.SetRenderTarget(_canvas);
            Profiler.EndSample();
            
            GL.Clear(true, true, Color.clear);
            
            Profiler.BeginSample("CreateMatrix");
            float rollRad = _camera.RotationRad.roll;
            quaternion rotation = new QuaternionEulerRad(BMath.PI * 0.5f - _camera.RotationRad.pitch, rollRad, 0);
            
            float4x4 viewMatrix = new Float4x4TRS
            {
                translation = _camera.Position.X0Y() + rotation.Mul(new float3(0, 0, _camera.ViewDistance)),
                scale = 1,
                rotation = rotation
            };
            
            viewMatrix = math.inverse(viewMatrix);

            float rollSin = BMath.Sin(rollRad);
            float4x4 projectionMatrix = float4x4.Ortho(
                _camera.ViewDistance * _screen.Aspect * rollSin,
                _camera.ViewDistance * rollSin, 
                0.01f, 
                10000);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetCommandBufferParams");
            _commandBuffer.Clear();
            _commandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            _commandBuffer.SetRenderTarget(_canvas);
            Profiler.EndSample();
            
            Profiler.BeginSample("_commandBuffer.DrawMeshInstancedProcedural");
            _commandBuffer.DrawMeshInstancedProcedural(_config.quadMesh, 0, _shapeMaterial, 0, shapeCount);
            Profiler.EndSample();

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
                    translation = new float3(0, 0, RenderOrderZ.Gizmos)
                },
                mesh = _config.quadMesh
            });
            Profiler.EndSample();
        }

        private RenderTexture _createCanvas(int2 screenSize)
        {
            return BGraphic.CreatePermRenderTexture(this, new RenderTextureData
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
        }
        
        private static float4x4 _сreateViewMatrix(float3 cameraPosition, quaternion cameraRotation)
        {
            float3 forward = math.mul(cameraRotation, new float3(0, 0, -1)); 
            float3 right = math.mul(cameraRotation, new float3(1, 0, 0));  
            float3 up = math.mul(cameraRotation, new float3(0, 1, 0));

            float4x4 viewMatrix = new (
                new float4(right, 0),
                new float4(up, 0),
                new float4(-forward, 0),
                new float4(-math.dot(right, cameraPosition),
                    -math.dot(up, cameraPosition),
                    math.dot(forward, cameraPosition),
                    1)
            );

            return viewMatrix;
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