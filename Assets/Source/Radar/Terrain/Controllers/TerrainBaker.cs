using Beast.Claw;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
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
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }
        
        private UniTask<OperationResult> _warmup()
        {
            _material = BGraphic.CreateMaterial(this, _graphicLibrary.terrain.baker);
            
            return OperationResult.SuccessSync();
        }

        public void Render(TerrainBakeTask task)
        {
            task.ValidateThrowException();
            
            _material.SetFloat("_Water01", task.terrain.water01);
            _material.SetFloat("_Forest01", task.terrain.water01);
            _material.SetFloat("_Seed", task.terrain.seed);
            _material.SetFloat("_BiomeSize01", task.terrain.biomeSize01);
            _material.SetFloat2("_TextureSize", task.outputTexture.Size());
            _material.SetFloat("_Quality01", task.quality01);
            
            BGraphic.Blit(new BlitParameters
            {
                targetTexture = task.outputTexture,
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