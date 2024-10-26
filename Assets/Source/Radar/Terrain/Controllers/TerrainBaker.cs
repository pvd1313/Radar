using Beast.Claw;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

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
        
        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            _material = BGraphic.CreateMaterial(this, _graphicLibrary.terrain.baker);
            
            return OperationResult.SuccessSync();
        }

        public void Render(TerrainBakeTask task)
        {
            task.ValidateThrowException(nameof(task));

            Random random = new (task.terrain.seed);
            float seedX = ((float)random.NextDouble() + 0.01f) * 10f - 5f;
            float seedY = seedX * -1f;
            
            _material.SetFloat("_Water01", task.terrain.water01);
            _material.SetFloat("_Forest01", task.terrain.forest01);
            _material.SetVector("_Seed", new Vector4(seedX, seedY));
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
        
        private UniTask<OperationResult> _dispose(OperationContext context)
        {
            BGraphic.TryDispose(ref _material);

            return OperationResult.SuccessSync();
        }
    }
}