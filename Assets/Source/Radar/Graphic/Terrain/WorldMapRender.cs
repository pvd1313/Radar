using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class WorldMapRender : IGameCycleListener
    {
        private static readonly int2 WorldSize = new(4096, 4096);
        
        private readonly ITerrainTexture _terrainTexture;
        private readonly GraphicLibrary _graphicLibrary;
        private readonly ICameraController _camera;

        private Material _renderMaterial;
        private Material _unlitMaterial;

        public WorldMapRender(
            ITerrainTexture terrainTexture, 
            GraphicLibrary graphicLibrary,
            ICameraController camera)
        {
            _terrainTexture = terrainTexture;
            _graphicLibrary = graphicLibrary;
            _camera = camera;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
            handle.ListenUpdate(this, UpdateID.FrameEnd, FrameEnd);
        }

        private UniTask<OperationResult> Warmup()
        {
            Application.targetFrameRate = 4064;

            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _graphicLibrary.worldMapRenderShader,
            });

            _unlitMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Unlit",
                shader = _graphicLibrary.debug.unlitColorShader
            });
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> Dispose()
        {
            if (_renderMaterial != null)
            {
                BeastCG.ReleaseMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            return OperationResult.SuccessSync();
        }

        private void FrameEnd()
        {
            if (!_terrainTexture.IsReady)
            {
                return;
            }
            
            BDebug.TrackFloat2(this, "cameraPosition", _camera.Position);
            BDebug.TrackFloat(this, "viewDistance", _camera.ViewDistance);
            
            float screenAspect = Screen.width / (float)Screen.height;
            
            _renderMaterial.SetFloat("_RenderAspect", screenAspect);
            _renderMaterial.SetFloat("_RenderSizeY", _camera.ViewDistance);
            _renderMaterial.SetVector("_RenderCenter", _camera.Position);
            
            _renderMaterial.SetTexture("_MainMap", _terrainTexture.Texture);
            _renderMaterial.SetVector("_MainMapSize", WorldSize);
            
#if BEAST_DEVELOPMENT
            _renderMaterial.SetTexture("_GridMap", _graphicLibrary.debug.worldGrid);
            _renderMaterial.SetVector("_GridMapSize", _graphicLibrary.debug.worldGrid.WidthHeightFloat());
#endif
            
            Texture2D worldDetailNoise = _graphicLibrary.worldDetailNoise;
            _renderMaterial.SetTexture("_NoiseMap", worldDetailNoise);
            _renderMaterial.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeightFloat());

            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _graphicLibrary.quadMesh,
                material = _renderMaterial,
                matrix = new Float4x4TRS
                {
                    translation = new float3(0, 0, 1),
                    rotation = new QuaternionEulerDeg(0, 0, 0),
                    scale = new float3(screenAspect, 1, 1)
                }
            });
            
            BDebug.TrackFloat2(this, "screenSize", new float2(Screen.width, Screen.height));

            _unlitMaterial.color = Color.red;
            
            float squareSize = (1 + screenAspect) / BMath.Cos(45 * BMath.Deg2Rad);
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _graphicLibrary.quadMesh,
                material = _unlitMaterial,
                matrix = new Float4x4TRS
                {
                    translation = new float3(0, 0, 1f),
                    rotation = new QuaternionEulerDeg(0, 0, 45),
                    scale = new float3(squareSize * 0.5f, squareSize * 0.5f, 1)
                }
            });
        }
    }
}