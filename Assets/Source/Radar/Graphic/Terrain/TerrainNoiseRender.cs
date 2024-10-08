using Beast.Claw.Graphic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class TerrainNoiseRender : ITerrainNoiseRender, IGameCycleListener
    {
        public bool IsReady => _isReady && _terrainTexture.IsReady;
        public Texture RenderedTexture => _renderedTexture;
        
        private readonly GameModel _game;
        private readonly IScreen _screen;
        private readonly ITerrainBakedTexture _terrainTexture;
        private readonly ITerrainBaker _baker;
        private readonly GraphicLibrary _library;
        private readonly ICameraController _camera;

        private Material _renderMaterial;
        private RenderTexture _renderedTexture;
        private float2 _renderCenter;
        private float _renderDistance;
        private bool _isReady;

        public TerrainNoiseRender(
            GameModel game,
            IScreen screen,
            ITerrainBakedTexture terrainTexture, 
            ITerrainBaker baker,
            GraphicLibrary library,
            ICameraController camera)
        {
            _game = game;
            _screen = screen;
            _terrainTexture = terrainTexture;
            _baker = baker;
            _library = library;
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
            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _library.terrainNoiseRenderShader,
            });
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> Dispose()
        {
            _isReady = false;
            
            if (_renderMaterial != null)
            {
                BeastCG.DisposeMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            if (_renderedTexture != null)
            {
                BeastCG.DisposeRenderTexture(_renderedTexture);
                _renderedTexture = null;
            }

            return OperationResult.SuccessSync();
        }

        private void FrameEnd()
        {
            _isReady = false;
            
            if (!_terrainTexture.IsReady)
            {
                return;
            }

            int2 textureSize = _screen.WidthHeight;

            float renderScale = 1;

            if (_renderedTexture == null || 
                _renderedTexture.width != textureSize.x || 
                _renderedTexture.height != textureSize.y)
            {
                if (_renderedTexture != null)
                {
                    BeastCG.DisposeRenderTexture(_renderedTexture);
                    _renderedTexture = null;
                }

                _renderedTexture = _baker.Create(this, textureSize, false);
            }

            float2x2 scale = IsometricCameraRotation.GetScaleMatrix(_camera.RotationRad.pitch);
            float2x2 rotation = IsometricCameraRotation.GetRotationMatrix(_camera.RotationRad.roll);

            _renderCenter = rotation.Multiply(scale.Multiply(_camera.Position));
            _renderDistance = _camera.ViewDistance * renderScale;
            
            _renderMaterial.SetFloat("_OutputAspect", _screen.Aspect);
            _renderMaterial.SetFloat("_OutputHeight", _renderDistance);
            _renderMaterial.SetVector("_OutputCenter", _renderCenter);
            
            _renderMaterial.SetTexture("_TerrainTexture", _terrainTexture.Texture);
            _renderMaterial.SetVector("_TerrainTextureSize", _game.terrain.size);
            
            _renderMaterial.SetMatrix("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetMatrix("_UVMatrix1", rotation.Inverse());
            
#if BEAST_DEVELOPMENT
            _renderMaterial.SetTexture("_DebugGridTexture", _library.debug.terrainGrid);
            _renderMaterial.SetVector("_DebugGridTextureSize", _library.debug.terrainGrid.WidthHeightFloat());
#endif
            
            Texture2D worldDetailNoise = _library.worldNoiseTexture;
            _renderMaterial.SetTexture("_NoiseMap", worldDetailNoise);
            _renderMaterial.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeightFloat());
            
            BeastCG.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = _renderedTexture
            });

            _isReady = true;
        }
    }
}