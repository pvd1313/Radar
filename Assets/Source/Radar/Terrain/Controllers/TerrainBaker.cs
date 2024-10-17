using Beast.Claw;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBaker : ITerrainBaker, IGameCycleListener
    {
        private readonly GraphicLibrary _graphicLibrary;

        private Material _material;

        public TerrainBaker(GraphicLibrary graphicLibrary)
        {
            _graphicLibrary = graphicLibrary;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
        }
        
        private UniTask<OperationResult> _warmup()
        {
            _material = BGraphic.CreateMaterial(this, _graphicLibrary.terrain.baker);
            
            return OperationResult.SuccessSync();
        }

        public void Bake(TerrainData terrain, RenderTexture targetTexture)
        {
            terrain.ValidateThrowException();
            targetTexture.ValidateNotNullThrowException(nameof(targetTexture));
            
            _material.SetFloat("_Flood01", terrain.waterFlood01);
            _material.SetFloat("_Seed01", terrain.seed01);
            _material.SetFloat("_Scale99", terrain.biomeScale99);
            
            _material.SetVector(
                "_TextureSize", 
                new Vector2(targetTexture.width, targetTexture.height));
            
            BGraphic.Blit(new BlitParameters
            {
                targetTexture = targetTexture,
                material = _material,
                shaderPass = 0
            });
        }
        
        private UniTask<OperationResult> _dispose()
        {
            BGraphic.TryDispose(ref _material);

            return OperationResult.SuccessSync();
        }
    }
}