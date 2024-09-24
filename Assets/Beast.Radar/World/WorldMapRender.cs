using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.CG;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace Beast.Radar.World
{
    public class WorldMapRender : Initializable, IFrameEndUpdate
    {
        private readonly WorldMapConfig _config;
        private readonly IWorldMapBaker _mapBaker;
        private readonly ShaderLibrary _shaders;
        private readonly MeshLibrary _meshes;

        private RenderTexture _worldTexture;
        private Material _renderMaterial;

        public WorldMapRender(
            WorldMapConfig config, 
            IWorldMapBaker mapBaker, 
            ShaderLibrary shaders,
            MeshLibrary meshes)
        {
            _config = config;
            _mapBaker = mapBaker;
            _shaders = shaders;
            _meshes = meshes;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            BeastLog.Info(this, BeastCG.GetSupportedGraphicsFormats());
            
            _worldTexture = BeastCG.NewRenderTexture(this, new RenderTextureParameters
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
            
            _renderMaterial = BeastCG.NewMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _config.mapRenderShader,
                mainTexture = _worldTexture
            });
            
            return InitializationResult.SuccessNotAsync();
        }

        public void OnFrameEnd()
        {
            float screenAspect = Screen.width / (float)Screen.height;
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _meshes.quad,
                material = _renderMaterial,
                matrix = float4x4.TRS(
                    float3.zero,  
                    BMath.Quaternion.EulerDeg(90, 0, 0),
                    new float3(screenAspect, 1, 1))
            });
        }
    }
}