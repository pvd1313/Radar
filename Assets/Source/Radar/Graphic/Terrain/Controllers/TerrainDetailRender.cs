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
        public Texture Texture => _detailTexture;
        public float2 TexelSize => _texelSize;

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
        private float _chunkSize;
        private float _texelSize;
        
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
            handle.ListenOperation(this, OperationID.Warmup, _Warmup);
            handle.ListenOperation(this, OperationID.Dispose, _Dispose);
            handle.ListenUpdate(this, UpdateID.FrameEnd, _FrameEnd);
        }

        private UniTask<OperationResult> _Warmup()
        {
            _Dispose();
            
            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _library.terrainDetailShader,
            });

            _renderTextureForScreenSize = _screen.Size;

            float cos60 = BMath.Cos(_camera.RotationRad.roll);
            float sin45 = BMath.Sin(_camera.RotationRad.pitch);
            int textureSize = BMath.CeilToInt(BMath.Max(_screen.Width, _screen.Height) / cos60 * sin45);
            _detailTexture = _baker.Create(this, textureSize, false);
            _textureYScale = textureSize / (float)_screen.Height;
            
            return OperationResult.SuccessSync();
        }
        
        public float2 WorldToUVOffset(float2 worldPoint)
        {
            float renderDistance = _camera.ViewDistance * _textureYScale;
            float2 offset = (worldPoint - _camera.Position) / renderDistance * 2;
            
            return (offset + 1) * 0.5f;
        }

        private void _FrameEnd()
        {
            _isReady = false;
            
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                _Dispose();
                _Warmup();
            }

            float renderDistance = _camera.ViewDistance * _textureYScale;
            _chunkSize = 1f / renderDistance;
            _texelSize = _chunkSize / GlobalConfig.CellTileCount;

            if (_chunkSize > GlobalConfig.CellTileCount)
            {
                renderDistance *= _texelSize;
            }
            
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
            
            _DebugTrack();
        }

        private void _DebugTrack()
        {
            BDebug.Track(this, "textureSize", _detailTexture.height);
            BDebug.Track(this, "textureYScale", _textureYScale);
            BDebug.Track(this, "chunkSize", _chunkSize);
            BDebug.Track(this, "texelSize", _texelSize);
        }
        
        private UniTask<OperationResult> _Dispose()
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
    }
}