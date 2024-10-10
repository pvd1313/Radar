using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainColorRender : IGameCycleListener
    {
        private readonly IScreen _screen;
        private readonly GraphicLibrary _library;
        private readonly ITerrainDetailRender _terrain;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;

        public TerrainColorRender(
            IScreen screen,
            GraphicLibrary library, 
            ITerrainDetailRender terrain,
            ICameraTransform camera)
        {
            _screen = screen;
            _library = library;
            _terrain = terrain;
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
            Application.targetFrameRate = 999;

            _renderMaterial = BeastCG.CreateMaterial(this, _library.terrainColorShader);

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            if (_renderMaterial != null)
            {
                BeastCG.DisposeMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            return OperationResult.SuccessSync();
        }
        
        private void FrameEnd()
        {
            if (!_terrain.IsReady)
            {
                return;
            }
            
            float2 renderTextureSize = _terrain.RenderTexture.SizeFloat();
            
            float yScale = BMath.Cos(_camera.CurrentRotationRad.roll);

            float2x2 rotation = float2x2.Rotate(-_camera.CurrentRotationRad.roll);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(_camera.CurrentRotationRad.pitch));
            
            BDebug.TrackFloat(this, "yScale", yScale);
            
            _renderMaterial.SetTexture("_TerrainTexture", _terrain.RenderTexture);
            _renderMaterial.SetVector("_TerrainTextureSize", renderTextureSize);
            
            _renderMaterial.SetMatrix("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetMatrix("_UVMatrix1", rotation.Inverse());
            
            _renderMaterial.SetFloat("_OutputAspect", _screen.Aspect);
            _renderMaterial.SetFloat("_OutputHeight", _screen.Height * yScale);
            _renderMaterial.SetVector("_OutputCenter", renderTextureSize * 0.5f);
            
            BeastCG.DrawMesh(new DrawMeshParameters
            {
                mesh = _library.quadMesh,
                material = _renderMaterial,
                matrix = new Float4x4TRS
                {
                    translation = new float3(0, 0, 1),
                    rotation = new QuaternionEulerDeg(0, 0, 0),
                    scale = new float3(_screen.Aspect, 1, 1)
                }
            });
        }
    }
}