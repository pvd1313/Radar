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
            Application.targetFrameRate = 999;

            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.colorRender);

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (!_biomeRender.IsReady)
            {
                return;
            }
            
            float2 renderTextureSize = _biomeRender.Texture.Size();
            float yScale = BMath.Cos(_camera.RotationRad.roll) * (_camera.ViewDistance / _biomeRender.ViewDistance);
            float2x2 rotation = float2x2.Rotate(-_camera.RotationRad.roll);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(_camera.RotationRad.pitch));
            
            _renderMaterial.SetTexture("_TerrainBiomeTexture", _biomeRender.Texture);
            _renderMaterial.SetFloat2("_TerrainBiomeTextureSize", renderTextureSize);

            TerrainAtlas atlas = _library.terrain.atlas.data;
            _renderMaterial.SetTexture("_TileAtlasColorTexture", atlas.colorTexture);
            _renderMaterial.SetFloat2("_TileAtlasTileCount", atlas.tileCount);
            
            _renderMaterial.SetFloat2x2("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetFloat2x2("_UVMatrix1", rotation.Inverse());

            _renderMaterial.SetFloat2("_OutputSize", _screen.Size.Multiply(yScale));

            float2 outputCenter = _biomeRender.WorldToUVOffset(_camera.Position);
            _renderMaterial.SetFloat2("_OutputCenter", renderTextureSize * outputCenter);
            
            BGraphic.DrawMesh(new DrawMeshParameters
            {
                mesh = _library.quad,
                material = _renderMaterial,
                matrix = new Float4x4TRS
                {
                    translation = new float3(0, 0, RenderOrderZ.TerrainColorRender),
                    rotation = quaternion.identity,
                    scale = new float3(_screen.Aspect, 1, 1)
                }
            });
        }
        
        private UniTask<OperationResult> _dispose()
        {
            BGraphic.TryDispose(ref _renderMaterial);

            return OperationResult.SuccessSync();
        }
    }
}