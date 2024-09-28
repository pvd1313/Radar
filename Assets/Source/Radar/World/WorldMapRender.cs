using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.CG;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Beast.Radar.World
{
    public class WorldMapRender : Initializable, IFrameEndUpdate
    {
        private static readonly int2 WorldSize = new(4096, 4096);
        
        private readonly WorldMapConfig _config;
        private readonly IWorldMapBaker _mapBaker;
        private readonly CGLibrary _cgLibrary;
        private readonly ICameraController _camera;
        private readonly CameraConfig _cameraConfig;

        private RenderTexture _worldTexture;
        private Material _material;

        public WorldMapRender(
            WorldMapConfig config, 
            IWorldMapBaker mapBaker, 
            CGLibrary cgLibrary,
            ICameraController camera,
            CameraConfig cameraConfig)
        {
            _config = config;
            _mapBaker = mapBaker;
            _cgLibrary = cgLibrary;
            _camera = camera;
            _cameraConfig = cameraConfig;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            Application.targetFrameRate = 4064;
            
            _worldTexture = BeastCG.NewRenderTexturePermanent(this, new NewPermaRenderTextureData
            {
                size = WorldSize,
                depth = 0,
                format = RenderTextureFormat.R8,
                mipCount = 0,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "WorldMap"
            });
            
            _mapBaker.Bake(new WorldMapBakerInput
            {
                scale99 = 2,
                seed01 = Random.value,
                targetTexture = _worldTexture,
                waterFlood01 = 0.5f
            });
            
            _material = BeastCG.NewMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _cgLibrary.worldMapRenderShader,
            });
            
            return InitializationResult.SuccessNotAsync();
        }

        public void OnFrameEnd()
        {
            BeastLog.Float2(this, "cameraPosition", _camera.State.position);
            
            float screenAspect = Screen.width / (float)Screen.height;
            float viewDistance = BMath.Lerp(_cameraConfig.zoomRange, _camera.State.zoom01);
            
            _material.SetFloat("_RenderAspect", screenAspect);
            _material.SetFloat("_RenderSizeY", viewDistance);
            _material.SetVector("_RenderCenter", _camera.State.position);
            
            _material.SetTexture("_MainMap", _worldTexture);
            _material.SetVector("_MainMapSize", WorldSize);
            
            _material.SetTexture("_GridMap", _cgLibrary.debug.worldGrid);
            _material.SetVector("_GridMapSize", _cgLibrary.debug.worldGrid.WidthHeight());
            
            Texture2D worldDetailNoise = _cgLibrary.worldDetailNoise;
            _material.SetTexture("_NoiseMap", worldDetailNoise);
            _material.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeight());

            float mapChunkSize = 100;
            float chunkTexelSize = Screen.height / (viewDistance * mapChunkSize);

            BeastLog.Float(this, "chunkTexelSize", chunkTexelSize);
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _cgLibrary.quadMesh,
                material = _material,
                matrix = float4x4.TRS(
                    new float3(0, 0, 1),  
                    BMath.Quaternion.EulerDeg(0, 0, 0),
                    new float3(screenAspect, 1, 1))
            });
        }
    }
}