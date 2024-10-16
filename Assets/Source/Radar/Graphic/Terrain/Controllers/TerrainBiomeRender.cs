using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainBiomeRender : ITerrainBiomeRender, IGameCycleListener
    {
        public bool IsReady => _isReady;
        public Texture Texture => _indexTexture;
        public float2 TexelSize => _texelSize;
        public float ViewDistance => _viewDistance;

        public float RenderDistance => _renderDistance;

        private readonly GameModel _game;
        private readonly IScreen _screen;
        private readonly ITerrainBakedTexture _bakedTerrain;
        private readonly GraphicLibrary _library;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;
        private RenderTexture _indexTexture;
        private int2 _renderTextureForScreenSize;
        private float _textureYScale;
        private float _texelSize;
        private float _cellPixelSize;
        private float _viewDistance;
        private float _renderDistance;
        private float _snapGridSize;
        private float2 _renderPosition;
        
        private bool _isReady;

        public TerrainBiomeRender(
            GameModel game,
            IScreen screen,
            ITerrainBakedTexture bakedTerrain,
            GraphicLibrary library,
            ICameraTransform camera)
        {
            _game = game;
            _screen = screen;
            _bakedTerrain = bakedTerrain;
            _library = library;
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
            _dispose();
            
            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.biomeRender);

            _renderTextureForScreenSize = _screen.Size;

            float cos60 = BMath.Cos(_camera.RotationRad.roll);
            float sin45 = BMath.Sin(_camera.RotationRad.pitch);
            int textureSize = BMath.CeilToInt(BMath.Max(_screen.Width, _screen.Height) / cos60 * sin45);
            
            // We need extra +2 space, because of grid snapping.
            textureSize = BMath.CeilToInt(textureSize / (float)WorldTransform.CellTileCount + 2) *
                          WorldTransform.CellTileCount;

            _indexTexture = BGraphic.CreatePermanentRenderTexture(this, new NewPermaRenderTextureData()
            {
                mipCount = 0,
                depth = 0,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                size = textureSize, 
                format = RenderTextureFormat.RHalf
            });
            
            _textureYScale = textureSize / (float)_screen.Height;
            
            return OperationResult.SuccessSync();
        }
        
        public float2 WorldToUVOffset(float2 worldPoint)
        {
            float2 offset = (worldPoint - _renderPosition) / _renderDistance * 2;
            
            return (offset + 1) * 0.5f;
        }

        private void _render()
        {
            _isReady = false;
            
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                _dispose();
                _warmup();
            }
            
            _viewDistance = _camera.ViewDistance;
            _cellPixelSize = _indexTexture.height / _viewDistance / _textureYScale;
            _renderPosition = _camera.Position;

            if (_cellPixelSize > WorldTransform.CellTileCount)
            {
                _viewDistance *= _cellPixelSize / WorldTransform.CellTileCount;
                _cellPixelSize = WorldTransform.CellTileCount;
            }

            _snapGridSize = WorldTransform.CellTileCount / _cellPixelSize;
            _renderPosition = BMath.Floor(_renderPosition / _snapGridSize) * _snapGridSize;
            _renderDistance = _viewDistance * _textureYScale;
            _texelSize = 1f / _renderDistance / WorldTransform.CellTileCount;
            
            _renderMaterial.SetFloat2("_OutputSize", _renderDistance);
            _renderMaterial.SetFloat2("_OutputCenter", _renderPosition);
            
            _renderMaterial.SetTexture("_TerrainTexture", _bakedTerrain.Texture);
            _renderMaterial.SetFloat2("_TerrainTextureSize", _game.terrain.size);
            
#if DEVELOPMENT
            _renderMaterial.SetTexture("_DebugGridTexture", _library.debug.terrainGrid);
            _renderMaterial.SetFloat2("_DebugGridTextureSize", _library.debug.terrainGrid.Size());
#endif
            
            _renderMaterial.SetTexture("_NoiseMap", _library.terrain.biomeRenderNoise);
            _renderMaterial.SetFloat2("_NoiseMapSize", _library.terrain.biomeRenderNoise.Size());
            
            BGraphic.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = _indexTexture
            });

            _isReady = true;
            
            _debugTrack();
        }
        
        private UniTask<OperationResult> _dispose()
        {
            _isReady = false;

            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _indexTexture);

            return OperationResult.SuccessSync();
        }
        
        private void _debugTrack()
        {
            BDebug.Track(this, "textureSize", _indexTexture.Size());
            BDebug.Track(this, nameof(_textureYScale), _textureYScale);
            BDebug.Track(this, nameof(_texelSize), _texelSize);
            BDebug.Track(this, nameof(_cellPixelSize), _cellPixelSize);
            BDebug.Track(this, nameof(_viewDistance), _viewDistance);
            BDebug.Track(this, nameof(_renderDistance), _renderDistance);
            BDebug.Track(this, nameof(_snapGridSize), _snapGridSize);
            BDebug.Track(this, nameof(_renderPosition), _renderPosition);
        }
    }
}