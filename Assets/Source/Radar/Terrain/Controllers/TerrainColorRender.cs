using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainColorRender : IGameCycleListener
    {
        private readonly IScreen _screen;
        private readonly GraphicLibrary _library;
        private readonly ITerrainBiomeRender _biomeRender;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;

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
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Render, _render);
        }

        private UniTask<OperationResult> _warmup()
        {
            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.colorRender);

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (!_biomeRender.IsReady || !_camera.IsReady)
            {
                return;
            }
            
            _renderColorTexture(new TerrainColorRenderInput
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

        private void _renderColorTexture(TerrainColorRenderInput input)
        {
            float yScale = BMath.Cos(input.cameraRotationRad.roll) * (input.cameraViewDistance / input.biomeViewDistance);
            float2x2 rotation = float2x2.Rotate(-input.cameraRotationRad.roll);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(input.cameraRotationRad.pitch));
            
            _renderMaterial.SetTexture("_TerrainBiomeTexture", input.biomeTexture);
            _renderMaterial.SetFloat2("_TerrainBiomeTextureSize", input.biomeTexture.Size());

            TerrainAtlas atlas = _library.terrain.atlas.data;
            _renderMaterial.SetTexture("_TileAtlasColorTexture", atlas.colorTexture);
            _renderMaterial.SetFloat2("_TileAtlasTileCount", atlas.tileCount);
            
            _renderMaterial.SetFloat2x2("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetFloat2x2("_UVMatrix1", rotation.Inverse());

            _renderMaterial.SetFloat2("_OutputSize", input.screenSize.Multiply(yScale));

            float2 outputCenter = input.biomeTextureUVOffset;
            _renderMaterial.SetFloat2("_OutputCenter", input.biomeTexture.Size() * outputCenter);

            float screenAspect = input.screenSize.x / (float)input.screenSize.y;

            if (input.outputToCamera)
            {
                BGraphic.DrawMesh(new DrawMeshParameters
                {
                    mesh = _library.quad,
                    material = _renderMaterial,
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
                throw new NotImplementedException();
            }
        }
        
        private UniTask<OperationResult> _dispose()
        {
            BGraphic.TryDispose(ref _renderMaterial);

            return OperationResult.SuccessSync();
        }
    }
}