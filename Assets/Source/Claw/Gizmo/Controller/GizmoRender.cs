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

        private const int PIXEL_PER_ENTITY = 2;
        private const int HALF_PER_PIXEL = 4;
        
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly IScreen _screen;
        [Required] private readonly GizmoConfig _config;

        private RenderTexture _canvas;
        private int2 _canvasSize;
        private List<HandleInfo> _handles;
        private Material _canvasMaterial;
        private Material _shapeMaterial;

        private HalfTextureProvider _halfTextureProvider;
        private HalfStream _halfStream;

        private CommandBuffer _commandBuffer;

        private int _prevShapeCount;

        public IGizmoHandle CreateHandle(IController owner)
        {
            owner.ValidateNotNullThrowException(nameof(owner));

            HandleInfo info = new ()
            {
                handle = new GizmoHandle(HALF_PER_PIXEL, PIXEL_PER_ENTITY),
                owner = owner,
            };

            _handles.Add(info);

            return info.handle;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _halfTextureProvider = new HalfTextureProvider(this, "DataTexture", HALF_PER_PIXEL);
            
            _handles = new List<HandleInfo>(64);
            
            _canvasMaterial = BGraphic.CreateMaterial(this, _config.canvasShader);
            _shapeMaterial = BGraphic.CreateMaterial(this, _config.shapeShader);
            _commandBuffer = BGraphic.CreateCommandBuffer(this, nameof(GizmoRender));

            _canvas = _setupCanvas(_screen.Size);

            _halfStream = HalfStream.CreateReadWrite(HALF_PER_PIXEL, PIXEL_PER_ENTITY, Allocator.Persistent, true);
            
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
            _halfStream.Clear();
            
            for (int i = 0; i < _handles.Count; i++)
            {
                HandleInfo handle = _handles[i];

                if (handle.handle.IsDisposed)
                {
                    _handles.RemoveAtSwapBack(i--);
                    continue;
                }

                if (handle.handle.halfStream.EntityCount == 0)
                {
                    continue;
                }

                _halfStream.AddStream(handle.handle.halfStream);
            }
            Profiler.EndSample();

            if (_halfStream.EntityCount == 0)
            {
                return;
            }

            int2 textureSize = _halfStream.GetRequiredTextureSize();

            Texture2D texture = _halfTextureProvider.GetTexture(textureSize);
            
            Profiler.BeginSample("SetPixelData");
            _halfStream.CopyToTexture(texture);
            Profiler.EndSample();

            Profiler.BeginSample("SetShapeDataTexture");
            _shapeMaterial.SetTexture("_DataTexture", texture);
            _shapeMaterial.SetFloat2("_DataTextureSize", textureSize);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetShapeMaterialMatrix");
            World2ClipMatrix matrix = new ()
            {
                screenSize = _screen.Size,
                cameraPosition = _camera.Position,
                cameraViewDistance = _camera.ViewDistance,
                cameraRotationRad = _camera.RotationRad,
                zOrder = RenderZOrder.GIZMO
            };

            matrix.Apply(_shapeMaterial);
            Profiler.EndSample();

            _setupMaterialsRare();
            
            Profiler.BeginSample("SetRenderTarget");
            Graphics.SetRenderTarget(_canvas);
            Profiler.EndSample();
            
            GL.Clear(true, true, Color.clear);

            if (_prevShapeCount != _halfStream.EntityCount)
            {
                Profiler.BeginSample("SetCommandBufferParams");
                _commandBuffer.Clear();
                _commandBuffer.SetRenderTarget(_canvas);
                _commandBuffer.DrawMeshInstancedProcedural(_config.quadMesh, 0, _shapeMaterial, 0, _halfStream.EntityCount);
                Profiler.EndSample();
                
                _prevShapeCount = _halfStream.EntityCount;
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
                    translation = new float3(0, 0, RenderZOrder.GIZMO)
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
            
            _canvasMaterial.SetTexture("_Texture", _canvas);

            Profiler.EndSample();
        }

        protected override void _dispose(BlockingContext context)
        {
            BGraphic.Dispose(_canvas);

            _halfStream.Dispose();
            _commandBuffer.Dispose();
            _halfTextureProvider.Dispose();

            BGraphic.Dispose(_canvasMaterial);
            BGraphic.Dispose(_shapeMaterial);
        }
    }
}