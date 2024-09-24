using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Radar.CG;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace Beast.Radar.World
{
    public class WorldMapRender : Initializable
    {
        private readonly WorldMapConfig _config;
        private readonly IWorldMapBaker _mapBaker;

        private RenderTexture _worldTexture;

        public WorldMapRender(WorldMapConfig config, IWorldMapBaker mapBaker)
        {
            _config = config;
            _mapBaker = mapBaker;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            _worldTexture = BeastCG.CreateRenderTexture(this, new RenderTextureParameters
            {
                size = new int2(512, 512),
                depth = 0,
                format = GraphicsFormat.R8_SNorm,
                mipCount = 0,
                name = "WorldMap"
            });

            _mapBaker.Bake(new WorldMapBakerInput
            {
                scale99 = 2,
                seed01 = Random.value,
                targetTexture = _worldTexture,
                waterFlood01 = 0.5f
            });
            
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);;
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            
            Material material = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _config.mapRenderShader,
                mainTexture = _worldTexture
            });

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
            
            return InitializationResult.SuccessNotAsync();
        }
    }
}