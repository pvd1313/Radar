using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Beast.Radar.Terrain
{
    public class TerrainFbmRender : Actor, ITerrainFbmRender
    {
        private readonly GraphicLibrary _graphicLibrary;

        private Material _material;

        public TerrainFbmRender(GraphicLibrary graphicLibrary)
        {
            _graphicLibrary = graphicLibrary;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _material = BGraphic.CreateMaterial(this, _graphicLibrary.terrain.fbmRender);
            
            return UniTask.CompletedTask;
        }

        public void Render(TerrainFbmRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            Random random = new (task.terrain.seed);
            float seedX = ((float)random.NextDouble() + 0.01f) * 10f - 5f;
            float seedY = seedX * -1f;

            float2 amount = new (task.terrain.water01, task.terrain.forest01);
            _material.SetFloat2("_Amount", amount);
            _material.SetFloat2("_Seed", new float2(seedX, seedY));
            _material.SetTexture("_GradientTexture", _graphicLibrary.terrain.bakerGradient);
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

        protected override void _dispose(BlockingContext context)
        {
            BGraphic.TryDispose(ref _material);
        }
    }
}