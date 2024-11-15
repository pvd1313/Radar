using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeRender : Controller, ITerrainBiomeRender, IGameLoopListener
    {
        private struct RenderResult
        {
            public float2 renderPosition;
            public float viewDistance;
            public float textureYScale;
            public float cellPixelSize;
            public float snapGridSize;
            public float renderDistance;
            public float texelSize;
            public float noiseSamplerLOD;
        }
        
        public bool IsReady => _isReady;
        
        public Texture Texture => _biomeTexture;
        public float2 TexelSize => _renderResult.texelSize;
        public float ViewDistance => _renderResult.viewDistance;

        private readonly IScreen _screen;
        private readonly ITerrainBakedFbm _bakedTerrain;
        private readonly GraphicLibrary _library;
        private readonly ICameraTransform _camera;
        private readonly GraphicConfig _config;

        private Material _renderMaterial;
        private RenderTexture _biomeTexture;
        private int2 _renderTextureForScreenSize;
        private RenderResult _renderResult;
        
        private bool _isReady;

        public TerrainBiomeRender(
            IScreen screen,
            ITerrainBakedFbm bakedTerrain,
            ICameraTransform camera,
            GraphicLibrary library,
            GraphicConfig config)
        {
            _screen = screen;
            _bakedTerrain = bakedTerrain;
            _library = library;
            _camera = camera;
            _config = config;
        }
        
        public void Register(IGameLoopHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.RENDER, _render);
        }

        public void Render(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException(nameof(task));
            
            _renderBiome(task);
        }
        
        public float2 WorldToUVOffset(float2 worldPoint)
        {
            float2 offset = (worldPoint - _renderResult.renderPosition) / _renderResult.renderDistance * 2;
            
            return (offset + 1) * 0.5f;
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.biomeRender);

            _renderTextureForScreenSize = _screen.Size;

            float cos60 = BMath.Cos(_camera.RotationRad.roll);
            float sin45 = BMath.Sin(_camera.RotationRad.pitch);
            int textureSize = BMath.CeilToInt(BMath.Max(_screen.Width, _screen.Height) / cos60 * sin45);
            
            // Will not work when aspect < 1.5
            textureSize = BMath.CeilToInt(textureSize / (float)WorldTransform.CellTileCount) *
                          WorldTransform.CellTileCount;

            RenderTextureData textureData = _config.terrain.biomeRenderTexture.WithSize(textureSize);
            _biomeTexture = BGraphic.CreatePermRenderTexture(this, textureData);

            return UniTask.CompletedTask;
        }
        
        private UniTask _gameLoad(ConcurrentContext context)
        {
            _renderResult.renderPosition = new float2(float.MaxValue, float.MaxValue);

            return UniTask.CompletedTask;
        }

        private void _render(BlockingContext context)
        {
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                _dispose(default);
                _warmup(default);
            }

            _isReady = false;
            
            _renderResult = _renderBiome(new TerrainBiomeRenderTask
            {
                outputBiomeTexture = _biomeTexture,
                bakedTerrain = _bakedTerrain.Texture,
                renderPosition = _camera.Position,
                screenSize = _screen.Size,
                viewDistance = _camera.ViewDistance,
                useDynamicConstraints = true,
            });
            
            _debugTrack(_renderResult);

            _isReady = true;
        }

        private RenderResult _renderBiome(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            RenderResult data;
            
            data.textureYScale = task.outputBiomeTexture.height / (float)task.screenSize.y;
            data.viewDistance = task.viewDistance;
            data.cellPixelSize = task.outputBiomeTexture.height / data.viewDistance / data.textureYScale;
            
            // Upscale when very close to terrain.
            if (task.useDynamicConstraints && data.cellPixelSize > WorldTransform.CellTileCount)
            {
                data.viewDistance *= data.cellPixelSize / WorldTransform.CellTileCount;
                data.cellPixelSize = WorldTransform.CellTileCount;
            }

            // Snap to grid.
            if (task.useDynamicConstraints)
            {
                data.snapGridSize = WorldTransform.CellTileCount / data.cellPixelSize;
                data.renderPosition = BMath.Floor(task.renderPosition / data.snapGridSize) * data.snapGridSize;
            }
            else
            {
                data.snapGridSize = 0;
                data.renderPosition = task.renderPosition;
            }
            
            data.renderDistance = data.viewDistance * data.textureYScale;
            data.texelSize = 1f / data.renderDistance / WorldTransform.CellTileCount;
            
            // LODs.
            if (!task.useDynamicConstraints || data.cellPixelSize < _config.terrain.noiseSamplerLOD1)
            {
                data.noiseSamplerLOD = 1;
            }
            else
            {
                FloatMinMax minMax = new(_config.terrain.noiseSamplerLOD1, _config.terrain.noiseSamplerLOD0);
                data.noiseSamplerLOD = BMath.Max(0, 1 - BMath.InverseLerp(minMax, data.cellPixelSize));
            }
            
            // Prevent rendering when texture not moving.
            if (task.useDynamicConstraints)
            {
                if (BMath.Approximately(data.renderPosition, _renderResult.renderPosition))
                {
                    BDebug.Track(this, "textureIsStatic", true);
                    return data;
                }
                
                BDebug.Track(this, "textureIsStatic", false);
            }
            
            _renderMaterial.SetFloat2("_OutputSize", data.renderDistance);
            _renderMaterial.SetFloat2("_OutputCenter", data.renderPosition);
            
            _renderMaterial.SetTexture("_TerrainTexture", task.bakedTerrain);
            _renderMaterial.SetFloat2("_TerrainTextureSize", task.bakedTerrain.Size());
            
            _renderMaterial.SetTexture("_IndexMap", _library.terrain.atlas.IndexTexture);

            _renderMaterial.SetTexture("_NoiseMap", _library.terrain.biomeRenderNoise);
            _renderMaterial.SetFloat2("_NoiseMapSize", _library.terrain.biomeRenderNoise.Size());

            _setNoisePassLOD(_renderMaterial, data.noiseSamplerLOD);

            BGraphic.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = task.outputBiomeTexture
            });

            return data;
        }

        private static void _setNoisePassLOD(Material material, float lod)
        {
            int lodInt = BMath.FloorToInt(lod);
            
            material.SetKeyword("NOISE_SAMPLER_LOD_0", lodInt == 0);
            material.SetKeyword("NOISE_SAMPLER_LOD_1", lodInt == 1);
            material.SetFloat("_NoiseSamplerLOD", lod);
        }
        
        private void _dispose(BlockingContext context)
        {
            _isReady = false;

            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _biomeTexture);
        }
        
        private void _debugTrack(RenderResult stack)
        {
            BDebug.Track(this, "biomeTextureSize", _biomeTexture.Size());
            BDebug.Track(this, "terrainTextureSize", _bakedTerrain.Texture.Size());
            BDebug.Track(this, "terrainTextureTexelSize", 1f / (float2)_biomeTexture.Size());
            BDebug.Track(this, nameof(stack.noiseSamplerLOD), stack.noiseSamplerLOD);
            BDebug.Track(this, nameof(stack.textureYScale), stack.textureYScale);
            BDebug.Track(this, nameof(stack.texelSize), stack.texelSize);
            BDebug.Track(this, nameof(stack.cellPixelSize), stack.cellPixelSize);
            BDebug.Track(this, nameof(stack.viewDistance), stack.viewDistance);
            BDebug.Track(this, nameof(stack.renderDistance), stack.renderDistance);
            BDebug.Track(this, nameof(stack.snapGridSize), stack.snapGridSize);
            BDebug.Track(this, nameof(stack.renderPosition), stack.renderPosition);
            BDebug.Track(this, nameof(stack.renderPosition), stack.renderPosition);
        }
    }
}