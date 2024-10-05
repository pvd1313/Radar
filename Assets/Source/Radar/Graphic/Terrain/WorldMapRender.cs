using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Beast.Radar.World
{
    public class WorldMapRender : IBeastInitialize, IBeastFrameEnd
    {
        private static readonly int2 WorldSize = new(4096, 4096);
        
        private readonly WorldMapConfig _config;
        private readonly ITerrainBaker _mapBaker;
        private readonly BeastGraphicLibrary _graphicLibrary;
        private readonly ICameraController _camera;
        private readonly CameraConfig _cameraConfig;

        private RenderTexture _worldTexture;
        private Material _material;

        public WorldMapRender(
            WorldMapConfig config, 
            ITerrainBaker mapBaker, 
            BeastGraphicLibrary graphicLibrary,
            ICameraController camera,
            CameraConfig cameraConfig)
        {
            _config = config;
            _mapBaker = mapBaker;
            _graphicLibrary = graphicLibrary;
            _camera = camera;
            _cameraConfig = cameraConfig;
        }
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            Application.targetFrameRate = 4064;
            
            _worldTexture = BeastCG.NewRenderTexturePermanent(this, new NewPermaRenderTextureData
            {
                size = WorldSize,
                depth = 0,
                format = RenderTextureFormat.RGFloat,
                mipCount = 0,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                name = "WorldMap"
            });
            
            _mapBaker.Bake(new TerrainBakerInput
            {
                scale99 = 2,
                seed01 = Random.value,
                targetTexture = _worldTexture,
                waterFlood01 = 0.5f
            });
            
            _material = BeastCG.NewMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _graphicLibrary.worldMapRenderShader,
            });
            
            return BeastInitializationResult.SuccessNotAsync();
        }

        public void OnFrameEnd()
        {
            BDebug.TrackFloat2(this, "cameraPosition", _camera.State.position);
            
            float screenAspect = Screen.width / (float)Screen.height;
            float viewDistance = BMath.Lerp(_cameraConfig.zoomRange, _camera.State.zoom01);
            
            _material.SetFloat("_RenderAspect", screenAspect);
            _material.SetFloat("_RenderSizeY", viewDistance);
            _material.SetVector("_RenderCenter", _camera.State.position);
            
            _material.SetTexture("_MainMap", _worldTexture);
            _material.SetVector("_MainMapSize", WorldSize);
            
            _material.SetTexture("_GridMap", _graphicLibrary.debug.worldGrid);
            _material.SetVector("_GridMapSize", _graphicLibrary.debug.worldGrid.WidthHeightFloat());
            
            Texture2D worldDetailNoise = _graphicLibrary.worldDetailNoise;
            _material.SetTexture("_NoiseMap", worldDetailNoise);
            _material.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeightFloat());

            float mapChunkSize = 100;
            float chunkTexelSize = Screen.height / (viewDistance * mapChunkSize);

            BDebug.TrackFloat(this, "chunkTexelSize", chunkTexelSize);
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _graphicLibrary.quadMesh,
                material = _material,
                matrix = float4x4.TRS(
                    new float3(0, 0, 1),  
                    BMath.Quaternion.EulerDeg(0, 0, 0),
                    new float3(screenAspect, 1, 1))
            });
        }
    }
}