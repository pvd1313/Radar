using System;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Radar.Terrain
{
    public class TerrainColorRender : Controller, IGameLoopListener, ITerrainColorRender
    {
        private readonly IScreen _screen;
        private readonly GraphicLibrary _library;
        private readonly ITerrainBiomeRender _biomeRender;
        private readonly ICameraTransform _camera;

        private Material _materialForCamera;

        public TerrainColorRender(
            IScreen screen,
            GraphicLibrary library, 
            ITerrainBiomeRender biomeRender,
            ICameraTransform camera)
        {
            _screen = screen;
            _library = library;
            _biomeRender = biomeRender;
            _camera = camera;
        }
        
        public void Register(IGameLoopHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            handle.ListenBlocking(ActionID.RENDER, _render);
        }
        
        public void Render(TerrainColorRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            if (task.outputToCamera)
            {
                string message =
                    "Rendering to camera is not supported. Because it will modify material that used in main render." +
                    "We need somehow to allocate and release materials after everything is rendered to implement this";

                throw new NotSupportedException(message);
            }
            
            // TODO: https://risenbeast.youtrack.cloud/issue/R-60/Render-material-pool
            Material material = BGraphic.CreateMaterial(this, _library.terrain.colorRender);

            _renderColorTexture(material, task);
            
            BGraphic.Dispose(material);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _materialForCamera = BGraphic.CreateMaterial(this, _library.terrain.colorRender);
            
            return UniTask.CompletedTask;
        }
        
        private void _render(BlockingContext context)
        {
            if (!_biomeRender.IsReady || !_camera.IsReady)
            {
                return;
            }
            
            _renderColorTexture(_materialForCamera, new TerrainColorRenderTask
            {
                biomeTexture = _biomeRender.Texture,
                biomeViewDistance = _biomeRender.ViewDistance,
                biomeTextureUVOffset = _biomeRender.WorldToUVOffset(_camera.Position),
                cameraViewDistance = _camera.ViewDistance,
                cameraRotationRad = _camera.RotationRad,
                outputToCamera = true,
                screenSize = _screen.Size
            });
        }

        private void _renderColorTexture(Material renderMaterial, TerrainColorRenderTask task)
        {
            renderMaterial.ValidateNotNullThrowException(nameof(renderMaterial));
            task.ValidateThrowException(nameof(task));
            
            renderMaterial.SetTexture("_TerrainBiomeTexture", task.biomeTexture);
            renderMaterial.SetFloat2("_TerrainBiomeTextureSize", task.biomeTexture.Size());

            TerrainAtlas atlas = _library.terrain.atlas.data;
            renderMaterial.SetTexture("_TileAtlasColorTexture", atlas.colorTexture);
            renderMaterial.SetFloat2("_TileAtlasTileCount", atlas.tileCount);
            
            float yScale = BMath.Cos(task.cameraRotationRad.roll) * (task.cameraViewDistance / task.biomeViewDistance);
            float2x2 rotation = float2x2.Rotate(task.cameraRotationRad.roll);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(task.cameraRotationRad.pitch));
            
            renderMaterial.SetFloat2x2("_UVMatrix0", scale.Inverse());
            renderMaterial.SetFloat2x2("_UVMatrix1", rotation);
            renderMaterial.SetFloat2("_OutputSize", task.screenSize.Mul(yScale));

            float2 outputCenter = task.biomeTextureUVOffset;
            renderMaterial.SetFloat2("_OutputCenter", task.biomeTexture.Size() * outputCenter);

            float screenAspect = task.screenSize.x / (float)task.screenSize.y;

            if (task.outputToCamera)
            {
                BGraphic.DrawMeshDelayed(new DrawMeshTask
                {
                    mesh = _library.quad,
                    material = renderMaterial,
                    matrix = new Float4x4TRS
                    {
                        translation = new float3(0, 0, RenderOrderZ.TerrainColorRender),
                        rotation = quaternion.identity,
                        scale = new float3(screenAspect, 1, 1)
                    }
                });
            }
            else
            {
                CommandBuffer commandBuffer = BGraphic.GetTempBuffer(this);
                float aspect = task.outputToTexture.width / (float)task.outputToTexture.height;
                float4x4 ortho = float4x4.Ortho(aspect, 1, 0, 1);
                commandBuffer.SetViewProjectionMatrices(float4x4.identity, ortho);
                commandBuffer.SetRenderTarget(task.outputToTexture);
                commandBuffer.DrawMesh(
                    _library.quad,
                    new Float4x4TRS
                    {
                        translation = float3.zero,
                        rotation = quaternion.identity,
                        scale = new float3(screenAspect, 1, 1)
                    },
                    renderMaterial,
                    0,
                    0);
                BGraphic.ExecuteBuffer(commandBuffer);
                BGraphic.DisposeTempBuffer(commandBuffer);
            }
        }
        
        private void _dispose(BlockingContext context)
        {
            BGraphic.TryDispose(ref _materialForCamera);
        }
    }
}