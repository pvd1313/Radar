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

        private Material _renderMaterial;
        private Material _solverMaterial;

        public TerrainTileRender(
            ICameraTransform camera, 
            GraphicLibrary library, 
            IScreen screen,
            ITerrainDetailRender terrain)
        {
            _camera = camera;
            _library = library;
            _screen = screen;
            _terrain = terrain;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
            handle.ListenUpdate(this, UpdateID.FrameEnd, FrameEnd);
        }

        private UniTask<OperationResult> Warmup()
        {
            _renderMaterial = BeastCG.CreateMaterial(this, _library.terrainTileRender);
            _solverMaterial = BeastCG.CreateMaterial(this, _library.terrainTileSolver);

            return OperationResult.SuccessSync();
        }
        
        private void FrameEnd()
        {
            if (!_terrain.IsReady)
            {
                return;
            }
            
            // if (Time.time % 1 > 0.5f)
            // {
            //     return;
            // }

            float tileHeight = 1f / _camera.ViewDistance / GlobalConfig.CellTileCount;
            BDebug.Track(this, "tileHeight", tileHeight);

            if (tileHeight < 1f / Screen.height)
            {
                return;
            }

            int yTileCount = BMath.CeilToInt(1f / tileHeight);
            int xTileCount = BMath.CeilToInt(1f / tileHeight * _screen.Aspect * 0.5f + 1);

            float2 viewportPoint = new float2(0, 1f);
            float2 worldPoint = _camera.ViewportToWorld(viewportPoint);
            
            int2 gridSize = new (xTileCount, yTileCount);
            
            int tileCount = TileCountFromGridSize(gridSize);
            
            BDebug.Track(this, "tileCount", tileCount);
            
            RenderTexture solverTexture = RunTileSolver(worldPoint, gridSize);
            
            RenderTileGrid(viewportPoint, gridSize, solverTexture, tileHeight);

            BeastCG.DisposeRenderTexture(solverTexture);
        }

        private RenderTexture RunTileSolver(float2 worldOrigin, int2 gridSize)
        {
            int tileCount = TileCountFromGridSize(gridSize);
            
            int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
            _solverMaterial.SetVector("_TileGridSize", gridSize);
            
            _solverMaterial.SetTexture("_TerrainTexture", _terrain.Texture);
            _solverMaterial.SetVector("_TerrainUVOffset", _terrain.WorldToUVOffset(worldOrigin));
            _solverMaterial.SetVector("_TerrainTexelSize", _terrain.TexelSize);
            
            _solverMaterial.SetTexture("_NoiseTexture", _library.worldNoiseTexture);
            
            // TODO : Not correct.
            _solverMaterial.SetVector("_NoiseUVOffset", _camera.Position);
            _solverMaterial.SetVector("_NoiseTexelSize", _camera.Position);
            
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

        private void RenderTileGrid(
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
        
            int tileCount = TileCountFromGridSize(gridSize);
            
            BDebug.Track(this, "outputOffset", viewportOrigin);
            
            Graphics.RenderMeshPrimitives(renderParams, _library.quadMesh, 0, tileCount);
        }

        private UniTask<OperationResult> Dispose()
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
        
        private static int TileCountFromGridSize(int2 gridSize)
        {
            return gridSize.x * gridSize.y + (gridSize.x - 1) * (gridSize.y - 1);
        }
    }
}