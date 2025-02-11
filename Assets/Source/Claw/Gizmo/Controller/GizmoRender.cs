using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Beast.Claw.Gizmo
{
    public class GizmoRender : Actor, IGizmoManager
    {
        private struct HandleInfo
        {
            public GizmoHandle handle;
            public IController owner;
        }

        private const int MAX_TEXTURE_SIZE = 128;
        private const int PIXEL_PER_ENTITY = 2;
        
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly IScreen _screen;
        [Required] private readonly GizmoConfig _config;

        private RenderTexture _canvas;
        private int2 _canvasSize;
        private List<HandleInfo> _handles;
        private Material _canvasMaterial;
        private Material _shapeMaterial;

        private Texture2D[] _texturePool;
        private PixelStream _pixelStream;

        private CommandBuffer _commandBuffer;

        private int _prevTexturePower;
        private int _prevShapeCount;

        public IGizmoHandle CreateHandle(IController owner)
        {
            owner.ValidateNotNullThrowException(nameof(owner));

            HandleInfo info = new ()
            {
                handle = new GizmoHandle(PIXEL_PER_ENTITY),
                owner = owner,
            };

            _handles.Add(info);

            return info.handle;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            Texture2DParameters textureData = new()
            {
                createInitialized = false,
                format = TextureFormat.RGBAHalf,
                isLinear = true,
                mipChain = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
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

            _pixelStream = new PixelStream(PIXEL_PER_ENTITY, Allocator.Persistent, true);
            
            return UniTask.CompletedTask;
        }

        protected override void _render(BlockingContext context)
        {
            if (!_canvasSize.Equals(_screen.Size))
            {
                BGraphic.Dispose(_canvas);

                _canvas = _setupCanvas(_screen.Size);
            }

            Profiler.BeginSample("UnionBuffers");
            _pixelStream.Clear();
            
            for (int i = 0; i < _handles.Count; i++)
            {
                HandleInfo handle = _handles[i];

                if (handle.handle.IsDisposed)
                {
                    _handles.RemoveAtSwapBack(i--);
                    continue;
                }

                if (handle.handle.pixelStream.EntityCount == 0)
                {
                    continue;
                }

                _pixelStream.AddEntitiesFrom(handle.handle.pixelStream);
            }
            Profiler.EndSample();

            if (_pixelStream.EntityCount == 0)
            {
                return;
            }

            int textureSize = _pixelStream.GetRequiredTextureSize();
            if (textureSize > MAX_TEXTURE_SIZE)
            {
                BDebug.LogError(this, $"Too big gizmo buffer: ({textureSize * textureSize})");
                return;
            }

            int texturePower2 = BMath.CeilToInt(BMath.Log2(textureSize));
            Texture2D texture = _texturePool[texturePower2];
                
            Profiler.BeginSample("SetPixelData");
            _pixelStream.CopyToTexture(texture);
            Profiler.EndSample();

            if (_prevTexturePower != texturePower2)
            {
                Profiler.BeginSample("SetShapeMaterialParams #2");
                _shapeMaterial.SetTexture("_DataTexture", texture);
                _shapeMaterial.SetFloat("_DataTextureSize", 1 << texturePower2);
                _prevTexturePower = texturePower2;
                Profiler.EndSample();
            }

            _setupMaterialsRare();
            
            Profiler.BeginSample("SetShapeMaterialParams #1");
            _shapeMaterial.SetFloat2("_RenderCenter", _camera.Position);
            _shapeMaterial.SetFloat("_RenderDistance", _camera.ViewDistance);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetRenderTarget");
            Graphics.SetRenderTarget(_canvas);
            Profiler.EndSample();
            
            GL.Clear(true, true, Color.clear);

            if (_prevShapeCount != _pixelStream.EntityCount)
            {
                Profiler.BeginSample("SetCommandBufferParams");
                _commandBuffer.Clear();
                _commandBuffer.SetRenderTarget(_canvas);
                _commandBuffer.DrawMeshInstancedProcedural(_config.quadMesh, 0, _shapeMaterial, 0, _pixelStream.EntityCount);
                Profiler.EndSample();
                _prevShapeCount = _pixelStream.EntityCount;
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
                    translation = new float3(0, 0, RenderOrderZ.GIZMO)
                },
                mesh = _config.quadMesh
            });
            Profiler.EndSample();
        }

        private RenderTexture _setupCanvas(int2 screenSize)
        {
            RenderTexture canvas = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
            {
                size = screenSize,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                format = RenderTextureFormat.ARGB32,
                antiAliasing = 1,
            });
            
            _canvasSize = screenSize;
            
            _setupMaterialsRare();

            return canvas;
        }

        private void _setupMaterialsRare()
        {
            Profiler.BeginSample("_setupMaterialsRare");
            
            _shapeMaterial.SetFloat2("_ScreenSize", _canvasSize);
            _canvasMaterial.SetTexture("_Texture", _canvas);

            if (_camera.RotationRad.IsZero())
            {
                _shapeMaterial.SetFloat2x2("_ViewMatrix0", float2x2.identity);
                _shapeMaterial.SetFloat2x2("_ViewMatrix1", float2x2.identity);
                _shapeMaterial.SetFloat("_ProjectionScale", 2f);
            }
            else
            {
                float rollCos = BMath.Cos(_camera.RotationRad.roll);
                float pitchCos = BMath.Cos(_camera.RotationRad.pitch);
            
                float2x2 rotation = float2x2.Rotate(_camera.RotationRad.roll * -1);
                float2x2 scale = float2x2.Scale(1, pitchCos);
            
                _shapeMaterial.SetFloat2x2("_ViewMatrix0", rotation);
                _shapeMaterial.SetFloat2x2("_ViewMatrix1", scale);
                _shapeMaterial.SetFloat("_ProjectionScale", 1f / (rollCos * pitchCos));
            }
            
            Profiler.EndSample();
        }

        protected override void _dispose(BlockingContext context)
        {
            BGraphic.Dispose(_canvas);

            foreach (Texture2D texture in _texturePool)
            {
                BGraphic.Dispose(texture);
            }

            _pixelStream.Dispose();
            _commandBuffer.Dispose();

            BGraphic.Dispose(_canvasMaterial);
            BGraphic.Dispose(_shapeMaterial);
        }
    }
}