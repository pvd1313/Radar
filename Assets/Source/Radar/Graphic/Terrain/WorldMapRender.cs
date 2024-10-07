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

        private Material _material;

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

            _material = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _graphicLibrary.worldMapRenderShader,
            });
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> Dispose()
        {
            if (_material != null)
            {
                BeastCG.ReleaseMaterial(_material);
                _material = null;
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
            
            _material.SetFloat("_RenderAspect", screenAspect);
            _material.SetFloat("_RenderSizeY", _camera.ViewDistance);
            _material.SetVector("_RenderCenter", _camera.Position);
            
            _material.SetTexture("_MainMap", _terrainTexture.Texture);
            _material.SetVector("_MainMapSize", WorldSize);
            
#if BEAST_DEVELOPMENT
            _material.SetTexture("_GridMap", _graphicLibrary.debug.worldGrid);
            _material.SetVector("_GridMapSize", _graphicLibrary.debug.worldGrid.WidthHeightFloat());
#endif
            
            Texture2D worldDetailNoise = _graphicLibrary.worldDetailNoise;
            _material.SetTexture("_NoiseMap", worldDetailNoise);
            _material.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeightFloat());

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