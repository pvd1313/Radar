using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.World
{
    public class TerrainColorRender : IGameCycleListener
    {
        private readonly IScreen _screen;
        private readonly GraphicLibrary _library;
        private readonly ITerrainNoiseRender _terrain;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;

        public TerrainColorRender(
            IScreen screen,
            GraphicLibrary library, 
            ITerrainNoiseRender terrain,
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
            Application.targetFrameRate = 4096;
            
            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                shader = _library.terrainColorRenderShader
            });

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

            float2 renderTextureSize = _terrain.RenderedTexture.WidthHeightFloat();

            BDebug.TrackFloat(this, "viewDistance", _camera.ViewDistance);

            float2x2 rotation = IsometricCameraRotation.GetRotationMatrix(_camera.RotationRad.roll);
            float2x2 scale = IsometricCameraRotation.GetScaleMatrix(_camera.RotationRad.pitch);
            
            _renderMaterial.SetTexture("_TerrainTexture", _terrain.RenderedTexture);
            _renderMaterial.SetVector("_TerrainTextureSize", renderTextureSize);

            _renderMaterial.SetMatrix("_UVMatrix0", rotation);
            _renderMaterial.SetMatrix("_UVMatrix1", scale);
            
            _renderMaterial.SetFloat("_OutputAspect", _screen.Aspect);
            _renderMaterial.SetFloat("_OutputHeight", _screen.Height);
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