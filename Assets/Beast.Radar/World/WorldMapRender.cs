using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.CG;
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
        private readonly MeshLibrary _meshes;
        private readonly ICameraController _camera;
        private readonly WorldCameraConfig _cameraConfig;

        private RenderTexture _worldTexture;
        private Material _material;

        public WorldMapRender(
            WorldMapConfig config, 
            IWorldMapBaker mapBaker, 
            MeshLibrary meshes,
            ICameraController camera,
            WorldCameraConfig cameraConfig)
        {
            _config = config;
            _mapBaker = mapBaker;
            _meshes = meshes;
            _camera = camera;
            _cameraConfig = cameraConfig;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            Application.targetFrameRate = 1000;
            
            _worldTexture = BeastCG.NewRenderTexture(this, new RenderTextureParameters
            {
                size = WorldSize,
                depth = 0,
                format = RenderTextureFormat.R8,
                mipCount = 0,
                filterMode = FilterMode.Point,
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
                shader = _config.mapRenderShader,
                mainTexture = _worldTexture
            });
            
            return InitializationResult.SuccessNotAsync();
        }

        public void OnFrameEnd()
        {
            float screenAspect = Screen.width / (float)Screen.height;
            float viewDistance = BMath.Lerp(_cameraConfig.zoomRange, _camera.State.zoom01);
            Vector2 cameraPosition = _camera.State.position;
            Vector2 worldSizeVector = new Vector2(WorldSize.x, WorldSize.y);
            
            _material.SetFloat(ShaderProperty.WorldMapRender._ScreenAspect, screenAspect);
            _material.SetFloat(ShaderProperty.WorldMapRender._ViewDistance, viewDistance);
            _material.SetVector(ShaderProperty.WorldMapRender._CameraPosition, cameraPosition);
            _material.SetVector(ShaderProperty.WorldMapRender._WorldTextureSize, worldSizeVector);
            _material.SetTexture(ShaderProperty.WorldMapRender._WorldTexture, _worldTexture);
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _meshes.quad,
                material = _material,
                matrix = float4x4.TRS(
                    float3.zero,  
                    BMath.Quaternion.EulerDeg(90, 0, 0),
                    new float3(screenAspect, 1, 1))
            });
        }
    }
}