using Beast.Claw.Attributes;
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
        [Required] private readonly GraphicLibrary _graphicLibrary;

        private Material _material;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _material = BGraphic.CreateMaterial(this, _graphicLibrary.terrain.fbmRender);
            
            return UniTask.CompletedTask;
        }

        public void Render(TerrainFbmRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            Random random = new (task.terrain.seed);
            float seed = (float)random.NextDouble();

            float2 amount = new (task.terrain.water01, task.terrain.forest01);
            _material.SetFloat2("_Amount", amount);
            _material.SetFloat("_Seed", seed);
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