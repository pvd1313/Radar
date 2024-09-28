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
        public static readonly int2 WorldSize = new(4096, 4096);
        
        
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
            Vector2 cameraPosition = _camera.State.position;
            Vector2 worldSizeVector = new (WorldSize.x, WorldSize.y);
            
            _material.SetFloat("_ScreenAspect", screenAspect);
            _material.SetFloat("_ViewDistance", viewDistance);
            _material.SetVector("_CameraPosition", cameraPosition);
            
            _material.SetVector("_WorldTextureSize", worldSizeVector);
            _material.SetTexture("_WorldTexture", _worldTexture);
            
            Texture2D worldDetailNoise = _cgLibrary.worldDetailNoise;
            _material.SetTexture("_WorldNoiseTexture", worldDetailNoise);
            _material.SetVector("_WorldNoiseTextureSize", (Vector2)worldDetailNoise.WidthHeight());

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