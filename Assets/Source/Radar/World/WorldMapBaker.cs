using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Radar.CG;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldMapBaker : Initializable, IWorldMapBaker
    {
        private readonly CGLibrary _cgLibrary;

        private Material _material;

        public WorldMapBaker(CGLibrary cgLibrary)
        {
            _cgLibrary = cgLibrary;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            _material = new Material(_cgLibrary.worldMapBakeShader);

            return InitializationResult.SuccessNotAsync();
        }

        public void Bake(WorldMapBakerInput input)
        {
            _material.SetFloat("_Flood01", input.waterFlood01);
            _material.SetFloat("_Seed01", input.seed01);
            _material.SetFloat("_Scale99", input.scale99);
            
            _material.SetVector(
                "_TextureSize", 
                new Vector2(input.targetTexture.width, input.targetTexture.height));
            
            BeastCG.Blit(new BlitParameters
            {
                targetTexture = input.targetTexture,
                material = _material,
                shaderPass = 0
            });
        }
    }
}