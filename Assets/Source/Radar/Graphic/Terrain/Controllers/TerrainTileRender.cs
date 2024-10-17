using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainTileRender : IGameCycleListener
    {
        private readonly ICameraTransform _camera;
        private readonly GraphicLibrary _library;
        private readonly IScreen _screen;
        private readonly ITerrainBiomeRender _terrainBiome;
        private readonly RenderConfig _config;

        private RenderTexture _tileToScreenBuffer;
        private Material _tileRenderMaterial;
        private Material _tileSolverMaterial;
        private Material _tileBlitMaterial;
        private float _tileHeight;
        private float _tilePixelSize;
        private int _tileCount;
        private int2 _gridSize;
        private float _blitTextureAlpha01;

        public TerrainTileRender(
            ICameraTransform camera, 
            GraphicLibrary library, 
            IScreen screen,
            ITerrainBiomeRender terrainBiome,
            RenderConfig config)
        {
            _camera = camera;
            _library = library;
            _screen = screen;
            _terrainBiome = terrainBiome;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Render, _render);
        }

        private UniTask<OperationResult> _warmup()
        {
            _tileRenderMaterial = BGraphic.CreateMaterial(this, _library.terrain.tileRender);
            _tileSolverMaterial = BGraphic.CreateMaterial(this, _library.terrain.tileSolver);
            _tileBlitMaterial = BGraphic.CreateMaterial(this, _library.terrain.tileBlit);
            _tileToScreenBuffer = BGraphic.CreatePermanentRenderTexture(this, new NewPermaRenderTextureData
            {
                filterMode = FilterMode.Point,
                depth = 0,
                format = RenderTextureFormat.ARGB32,
                wrapMode = TextureWrapMode.Clamp,
                size = _screen.Size,
                mipCount = 0,
            });

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (!_terrainBiome.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_tileToScreenBuffer.Size()))
            {
                _dispose();
                _warmup();
            }
            
            // if (Time.time % 1 > 0.5f)
            // {
            //     return;
            // }

            _tileHeight = 1f / _camera.ViewDistance / WorldTransform.CellTileCount;
            _tilePixelSize = _tileHeight * _screen.Height;
            if (_tilePixelSize < _config.terrain.tileMinPixelSize)
            {
                return;
            }

            int yTileCount = BMath.CeilToInt(1f / _tileHeight) + 2;
            int xTileCount = BMath.CeilToInt(1f / _tileHeight * _screen.Aspect * 0.5f) + 3;
            _gridSize = new int2(xTileCount, yTileCount);
            _tileCount = _tileCountFromGridSize(_gridSize);
            if (_tileCount > _config.terrain.tileMaxCount)
            {
                return;
            }
            
            float2 viewportPoint = new (0, 1.0f);
            float2 worldPoint = _camera.ViewportToWorld(viewportPoint);

            float cellTileCount = WorldTransform.CellTileCount;
            float texelSize = 1.0f / cellTileCount;
            worldPoint.x -= texelSize;
            worldPoint.y += texelSize * 2.0f;
            
            // Snap to the grid.
            worldPoint = BMath.Floor(worldPoint * cellTileCount) / cellTileCount + texelSize * 0.5f;
            viewportPoint = _camera.WorldToViewport(worldPoint);
            
            // Fade
            TerrainRenderConfig terrain = _config.terrain;
            float tileCountFade = BMath.ILerp(terrain.tileFadeInCount, terrain.tileMaxCount, _tileCount);
            float pixelSizeFade = BMath.ILerp(terrain.tileFadeInPixelSize, terrain.tileMinPixelSize, _tilePixelSize);
            _blitTextureAlpha01 = 1.0f - BMath.Clamp01(BMath.Max(tileCountFade, pixelSizeFade));
            
            RenderTexture solverTexture = _runTileSolver(worldPoint, _gridSize);
            
            _renderTileGridToBuffer(viewportPoint, _gridSize, solverTexture, _tileHeight);

            _blitTileBufferToScreen();
            
            _debugTrack();

            BGraphic.Dispose(solverTexture);
        }

        private RenderTexture _runTileSolver(float2 worldPoint, int2 gridSize)
        {
            int tileCount = _tileCountFromGridSize(gridSize);
            
            int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
            _tileSolverMaterial.SetFloat2("_TileGridSize", gridSize);

            TerrainAtlas atlas = _library.terrain.atlas.data;
            
            _tileSolverMaterial.SetTexture("_TileAtlasConfig", atlas.configTexture);
            _tileSolverMaterial.SetFloat2("_TileAtlasTileCount", atlas.tileCount);
            
            _tileSolverMaterial.SetTexture("_TerrainTexture", _terrainBiome.Texture);
            _tileSolverMaterial.SetFloat2("_TerrainUVOffset", _terrainBiome.WorldToUVOffset(worldPoint));
            _tileSolverMaterial.SetFloat2("_TerrainTexelSize", _terrainBiome.TexelSize);
            
            float2 noiseTexelSize = _terrainBiome.TexelSize * _config.terrain.tileDenoiseScale;
            float2 noiseUVOffset = worldPoint / _terrainBiome.RenderDistance * _config.terrain.tileDenoiseScale;
            
            _tileSolverMaterial.SetTexture("_NoiseTexture", _library.terrain.tileSolverNoise);
            _tileSolverMaterial.SetFloat2("_NoiseTexelSize",  noiseTexelSize);
            _tileSolverMaterial.SetFloat2("_NoiseUVOffset", noiseUVOffset);
            
            _tileSolverMaterial.SetFloat("_SolverTextureSize", solverTextureSize);
        
            RenderTexture solverTexture = BGraphic.CreateTemporaryRenderTexture(
                this, 
                new NewTempRenderTextureData
            {
                size = solverTextureSize,
                antiAliasing = 1,
                readWrite = RenderTextureReadWrite.Linear, 
                format = RenderTextureFormat.ARGBHalf,
                filterMode = FilterMode.Point
            });
        
            BGraphic.Blit(new BlitParameters
            {
                material = _tileSolverMaterial,
                shaderPass = 0,
                targetTexture = solverTexture
            });

            return solverTexture;
        }

        private void _renderTileGridToBuffer(
            float2 viewportOrigin, 
            int2 gridSize, 
            RenderTexture solverTexture, 
            float tileHeight)
        {
            // Isometric render (0, 0) is at top-left.
            viewportOrigin.y = 1 - viewportOrigin.y;

            TerrainAtlas atlas = _library.terrain.atlas.data;
            
            _tileRenderMaterial.SetFloat2("_TileGridSize", gridSize);
            _tileRenderMaterial.SetTexture("_TileAtlas", atlas.mainTexture);
            _tileRenderMaterial.SetFloat2("_TileAtlasWidthHeight", atlas.mainTexture.Size());
            _tileRenderMaterial.SetFloat2("_TileAtlasTileCount", atlas.tileCount);
            _tileRenderMaterial.SetFloat("_TileAtlasPPU", atlas.pixelsPerTile);
            _tileRenderMaterial.SetTexture("_SolverTexture", solverTexture);
            _tileRenderMaterial.SetFloat("_SolverSize", solverTexture.width);
            _tileRenderMaterial.SetFloat2("_OutputSize", _screen.Size);
            
            _tileRenderMaterial.SetFloat2("_OutputOffset", viewportOrigin);
            _tileRenderMaterial.SetFloat("_OutputTileHeight", tileHeight);
        
            int tileCount = _tileCountFromGridSize(gridSize);

            CommandBuffer commandBuffer = BGraphic.GetTempBuffer(this);
            commandBuffer.SetRenderTarget(_tileToScreenBuffer);
            commandBuffer.DrawMeshInstancedProcedural(_library.quad, 0, _tileRenderMaterial, 0, tileCount);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            BGraphic.DisposeTempBuffer(commandBuffer);
        }

        private void _blitTileBufferToScreen()
        {
            _tileBlitMaterial.SetFloat("_Alpha", _blitTextureAlpha01);
            _tileBlitMaterial.SetFloat2("_OutputSize", _screen.Size);
            _tileBlitMaterial.SetTexture("_MainTexture", _tileToScreenBuffer);
            
            BGraphic.DrawMesh(new DrawMeshParameters
            {
                mesh = _library.quad,
                material = _tileBlitMaterial, 
                matrix = new Float4x4TRS
                {
                    translation = new float3(0, 0, RenderOrderZ.TerrainTileRender),
                    rotation = quaternion.identity,
                    scale = new float3(_screen.Aspect, 1, 1)
                }
            });
        }
        
        private static int _tileCountFromGridSize(int2 gridSize)
        {
            return gridSize.x * gridSize.y + (gridSize.x - 1) * (gridSize.y - 1);
        }

        private UniTask<OperationResult> _dispose()
        {
            BGraphic.TryDispose(ref _tileBlitMaterial);
            BGraphic.TryDispose(ref _tileSolverMaterial);
            BGraphic.TryDispose(ref _tileRenderMaterial);
            BGraphic.TryDispose(ref _tileToScreenBuffer);

            return OperationResult.SuccessSync();
        }

        private void _debugTrack()
        {
            BDebug.Track(this, nameof(_tileHeight), _tileHeight);
            BDebug.Track(this, nameof(_tileCount), _tileCount);
            BDebug.Track(this, nameof(_gridSize), _gridSize);
            BDebug.Track(this, nameof(_blitTextureAlpha01), _blitTextureAlpha01);
            BDebug.Track(this, nameof(_tilePixelSize), _tilePixelSize);
        }
    }
}