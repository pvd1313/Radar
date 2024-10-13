using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
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
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Render, _render);
        }

        private UniTask<OperationResult> _warmup()
        {
            Application.targetFrameRate = 999;

            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrainColorShader);

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            if (_renderMaterial != null)
            {
                BGraphic.DisposeMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (!_terrain.IsReady)
            {
                return;
            }
            
            float2 renderTextureSize = _terrain.Texture.Size();
            float yScale = BMath.Cos(_camera.RotationRad.roll) * (_camera.ViewDistance / _terrain.ViewDistance);
            float2x2 rotation = float2x2.Rotate(-_camera.RotationRad.roll);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(_camera.RotationRad.pitch));
            
            _renderMaterial.SetTexture("_TerrainTexture", _terrain.Texture);
            _renderMaterial.SetVector("_TerrainTextureSize", renderTextureSize);
            
            _renderMaterial.SetMatrix("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetMatrix("_UVMatrix1", rotation.Inverse());
            
            _renderMaterial.SetFloat("_OutputAspect", _screen.Aspect);
            _renderMaterial.SetFloat("_OutputHeight", _screen.Height * yScale);

            float2 outputCenter = _terrain.WorldToUVOffset(_camera.Position);
            _renderMaterial.SetVector("_OutputCenter", renderTextureSize * outputCenter);
            
            BGraphic.DrawMesh(new DrawMeshParameters
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