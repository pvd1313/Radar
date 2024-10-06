using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldMapRender : IBeastGameCycleListener
    {
        private static readonly int2 WorldSize = new(4096, 4096);
        
        private readonly IBeastTerrainTexture _terrainTexture;
        private readonly BeastGraphicLibrary _graphicLibrary;
        private readonly IBeastCameraController _camera;

        private Material _material;

        public WorldMapRender(
            IBeastTerrainTexture terrainTexture, 
            BeastGraphicLibrary graphicLibrary,
            IBeastCameraController camera)
        {
            _terrainTexture = terrainTexture;
            _graphicLibrary = graphicLibrary;
            _camera = camera;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
            handle.ListenUpdate(this, BeastCycleEventID.FrameEnd, FrameEnd);
        }

        private UniTask<BeastInitializationResult> Warmup()
        {
            Application.targetFrameRate = 4064;

            _material = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _graphicLibrary.worldMapRenderShader,
            });
            
            return BeastInitializationResult.SuccessSync();
        }
        
        private UniTask<BeastInitializationResult> Dispose()
        {
            if (_material != null)
            {
                BeastCG.ReleaseMaterial(_material);
                _material = null;
            }

            return BeastInitializationResult.SuccessSync();
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
            
            _material.SetTexture("_GridMap", _graphicLibrary.debug.worldGrid);
            _material.SetVector("_GridMapSize", _graphicLibrary.debug.worldGrid.WidthHeightFloat());
            
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