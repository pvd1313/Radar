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
    public class TerrainTileRender : IGameCycleListener
    {
        private readonly ICameraTransform _camera;
        private readonly GraphicLibrary _library;
        private readonly IScreen _screen;
        private readonly ITerrainDetailRender _terrain;
        private readonly RenderConfig _config;

        private Material _renderMaterial;
        private Material _solverMaterial;
        private float _tileHeight;
        private int _tileCount;
        private int2 _gridSize;

        public TerrainTileRender(
            ICameraTransform camera, 
            GraphicLibrary library, 
            IScreen screen,
            ITerrainDetailRender terrain,
            RenderConfig config)
        {
            _camera = camera;
            _library = library;
            _screen = screen;
            _terrain = terrain;
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
            _renderMaterial = BeastCG.CreateMaterial(this, _library.terrainTileRender);
            _solverMaterial = BeastCG.CreateMaterial(this, _library.terrainTileSolver);

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (!_terrain.IsReady)
            {
                return;
            }
            
            // if (Time.time % 1 > 0.5f)
            // {
            //     return;
            // }

            _tileHeight = 1f / _camera.ViewDistance / WorldTransform.CellTileCount;
            if (_tileHeight < _config.terrainTilePixelSizeMin / (float)_screen.Height)
            {
                return;
            }

            int yTileCount = BMath.CeilToInt(1f / _tileHeight) + 2;
            int xTileCount = BMath.CeilToInt(1f / _tileHeight * _screen.Aspect * 0.5f) + 3;
            _gridSize = new int2(xTileCount, yTileCount);
            _tileCount = _tileCountFromGridSize(_gridSize);
            if (_tileCount > _config.terrainTileCountLimit)
            {
                return;
            }
            
            float2 viewportPoint = new (0, 1f);
            float2 worldPoint = _camera.ViewportToWorld(viewportPoint);

            float cellTileCount = WorldTransform.CellTileCount;
            float texelSize = 1f / cellTileCount;
            worldPoint.x -= texelSize;
            worldPoint.y += texelSize * 2;
            
            // Stick to the grid.
            worldPoint = BMath.Floor(worldPoint * cellTileCount) / cellTileCount + texelSize * 0.5f;
            viewportPoint = _camera.WorldToViewport(worldPoint);
            
            RenderTexture solverTexture = _runTileSolver(worldPoint, _gridSize);
            
            _renderTileGrid(viewportPoint, _gridSize, solverTexture, _tileHeight);

            _debugTrack();

            BeastCG.DisposeRenderTexture(solverTexture);
        }

        private RenderTexture _runTileSolver(float2 worldOrigin, int2 gridSize)
        {
            int tileCount = _tileCountFromGridSize(gridSize);
            
            int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
            _solverMaterial.SetVector("_TileGridSize", gridSize);
            
            _solverMaterial.SetTexture("_TerrainTexture", _terrain.Texture);
            _solverMaterial.SetVector("_TerrainUVOffset", _terrain.WorldToUVOffset(worldOrigin));
            _solverMaterial.SetVector("_TerrainTexelSize", _terrain.TexelSize);
            
            _solverMaterial.SetTexture("_NoiseTexture", _library.terrainTileSolverNoiseTexture);
            _solverMaterial.SetVector("_NoiseTextureSize", _library.terrainTileSolverNoiseTexture.Size());
            _solverMaterial.SetVector("_NoiseWorldOffset", worldOrigin);
            
            _solverMaterial.SetFloat("_SolverTextureSize", solverTextureSize);
        
            RenderTexture solverTexture = BeastCG.CreateTemporaryRenderTexture(
                this, 
                new NewTempRenderTextureData
            {
                size = solverTextureSize,
                antiAliasing = 1,
                readWrite = RenderTextureReadWrite.Linear, 
                format = RenderTextureFormat.RGHalf,
                filterMode = FilterMode.Point
            });
        
            BeastCG.Blit(new BlitParameters
            {
                material = _solverMaterial,
                shaderPass = 0,
                targetTexture = solverTexture
            });

            return solverTexture;
        }

        private void _renderTileGrid(
            float2 viewportOrigin, 
            int2 gridSize, 
            RenderTexture solverTexture, 
            float tileHeight)
        {
            // Isometric render (0, 0) is at top-left.
            viewportOrigin.y = 1 - viewportOrigin.y;
            
            RenderParams renderParams = new ()
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one),
                material = _renderMaterial,
                matProps = new MaterialPropertyBlock()
            };

            TerrainTileAtlas atlas = _library.terrainTileAtlas;
        
            renderParams.matProps.SetVector("_TileGridSize", gridSize);
            renderParams.matProps.SetTexture("_TileAtlas", atlas.atlasTexture);
            renderParams.matProps.SetVector("_TileAtlasWidthHeight", atlas.atlasTexture.Size());
            renderParams.matProps.SetVector("_TileAtlasTileCount", atlas.tileCount);
            renderParams.matProps.SetVector("_TileBlendCenter", atlas.tileBlendCenter);
            renderParams.matProps.SetFloat("_TileAtlasPPU", atlas.astlasPPU);
            renderParams.matProps.SetFloat("_RenderYCrop", atlas.renderYCrop);
            renderParams.matProps.SetTexture("_SolverTexture", solverTexture);
            renderParams.matProps.SetFloat("_SolverSize", solverTexture.width);
            renderParams.matProps.SetVector("_OutputSize", _screen.Size);
            
            renderParams.matProps.SetVector("_OutputOffset", viewportOrigin);
            renderParams.matProps.SetFloat("_OutputTileHeight", tileHeight);
        
            int tileCount = _tileCountFromGridSize(gridSize);
            
            Graphics.RenderMeshPrimitives(renderParams, _library.quadMesh, 0, tileCount);
        }
        
        private static int _tileCountFromGridSize(int2 gridSize)
        {
            return gridSize.x * gridSize.y + (gridSize.x - 1) * (gridSize.y - 1);
        }

        private UniTask<OperationResult> _dispose()
        {
            if (_renderMaterial != null)
            {
                BeastCG.DisposeMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            if (_solverMaterial != null)
            {
                BeastCG.DisposeMaterial(_solverMaterial);
                _solverMaterial = null;
            }

            return OperationResult.SuccessSync();
        }

        private void _debugTrack()
        {
            BDebug.Track(this, nameof(_tileHeight), _tileHeight);
            BDebug.Track(this, nameof(_tileCount), _tileCount);
            BDebug.Track(this, nameof(_gridSize), _gridSize);
        }
    }
}