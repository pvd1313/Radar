using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Radar.CG;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldMapBaker : Initializable, IWorldMapBaker
    {
        private readonly WorldMapConfig _config;

        private Material _material;

        public WorldMapBaker(WorldMapConfig config)
        {
            _config = config;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            _material = new Material(_config.mapFloodShader);

            return InitializationResult.SuccessNotAsync();
        }

        public void Bake(WorldMapBakerInput input)
        {
            _material.SetFloat(ShaderProperty.WorldMapFlood.Flood01, input.waterFlood01);
            _material.SetFloat(ShaderProperty.WorldMapFlood.Seed01, input.seed01);
            _material.SetFloat(ShaderProperty.WorldMapFlood.Scale99, input.scale99);
            
            Graphics.Blit(null, input.targetTexture, _material, 0);
        }
    }
}