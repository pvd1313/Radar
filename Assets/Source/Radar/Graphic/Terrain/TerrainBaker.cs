using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class TerrainBaker : IBeastInitialize, ITerrainBaker
    {
        private readonly BeastGraphicLibrary _graphicLibrary;

        private Material _material;

        public TerrainBaker(BeastGraphicLibrary graphicLibrary)
        {
            _graphicLibrary = graphicLibrary;
        }
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            _material = new Material(_graphicLibrary.worldMapBakeShader);

            return BeastInitializationResult.SuccessNotAsync();
        }

        public void Bake(TerrainBakerInput input)
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