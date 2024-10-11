using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainDetailRender : ITerrainDetailRender, IGameCycleListener
    {
        public bool IsReady => _isReady;
        public Texture DetailTexture => _detailTexture;

        private readonly GameModel _game;
        private readonly IScreen _screen;
        private readonly ITerrainBakedTexture _bakedTerrain;
        private readonly ITerrainBaker _baker;
        private readonly GraphicLibrary _library;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;
        private RenderTexture _detailTexture;
        private int2 _renderTextureForScreenSize;
        private float _textureYScale;
        
        private bool _isReady;

        public TerrainDetailRender(
            GameModel game,
            IScreen screen,
            ITerrainBakedTexture bakedTerrain, 
            ITerrainBaker baker,
            GraphicLibrary library,
            ICameraTransform camera)
        {
            _game = game;
            _screen = screen;
            _bakedTerrain = bakedTerrain;
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
            Dispose();
            
            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _library.terrainDetailShader,
            });

            _renderTextureForScreenSize = _screen.Size;

            float cos60 = BMath.Cos(_camera.RotationRad.roll);
            float sin45 = BMath.Sin(_camera.RotationRad.pitch);
            int textureSize = BMath.CeilToInt(BMath.Max(_screen.Width, _screen.Height) / cos60 * sin45);
            _detailTexture = _baker.Create(this, new int2(textureSize, textureSize), false);
            _textureYScale = textureSize / (float)_screen.Height;
            
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

            if (_detailTexture != null)
            {
                BeastCG.DisposeRenderTexture(_detailTexture);
                _detailTexture = null;
            }

            return OperationResult.SuccessSync();
        }

        private void FrameEnd()
        {
            _isReady = false;
            
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                Dispose();
                Warmup();
            }

            float renderDistance = _camera.ViewDistance * _textureYScale;
            float chunkSize = _camera.ViewDistance / renderDistance;

            if (chunkSize > 100)
            {
                renderDistance *= (chunkSize / 100);
            }
            
            BDebug.Track(this, "detailTextureSize", _detailTexture.height);
            BDebug.Track(this, "detailTextureYScale", _textureYScale);
            BDebug.Track(this, "chunkSize", chunkSize);
            
            _renderMaterial.SetFloat("_OutputAspect", 1);
            _renderMaterial.SetFloat("_OutputHeight", renderDistance);
            _renderMaterial.SetVector("_OutputCenter", _camera.Position);
            
            _renderMaterial.SetTexture("_TerrainTexture", _bakedTerrain.Texture);
            _renderMaterial.SetVector("_TerrainTextureSize", _game.terrain.size);
            
#if BEAST_DEVELOPMENT
            _renderMaterial.SetTexture("_DebugGridTexture", _library.debug.terrainGrid);
            _renderMaterial.SetVector("_DebugGridTextureSize", _library.debug.terrainGrid.Size());
#endif
            
            _renderMaterial.SetTexture("_NoiseMap", _library.worldNoiseTexture);
            _renderMaterial.SetVector("_NoiseMapSize", _library.worldNoiseTexture.Size());
            
            BeastCG.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = _detailTexture
            });

            _isReady = true;
        }
    }
}