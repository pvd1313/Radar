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
        private readonly ITerrainDetailRender _terrainDetail;
        private readonly ITerrainBakedTexture _terrainBaked;

        private Material _renderMaterial;
        private Material _solverMaterial;

        public TerrainTileRender(
            ICameraTransform camera, 
            GraphicLibrary library, 
            IScreen screen,
            ITerrainDetailRender terrainDetail,
            ITerrainBakedTexture terrainBaked)
        {
            _camera = camera;
            _library = library;
            _screen = screen;
            _terrainDetail = terrainDetail;
            _terrainBaked = terrainBaked;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
            handle.ListenUpdate(this, UpdateID.FrameEnd, FrameEnd);
        }

        private UniTask<OperationResult> Warmup()
        {
            _renderMaterial = BeastCG.CreateMaterial(this, _library.isometricRenderShader);
            _solverMaterial = BeastCG.CreateMaterial(this, _library.isometricSolverShader);

            return OperationResult.SuccessSync();
        }
        
        private void FrameEnd()
        {
            BDebug.TrackFloat2(this, "cameraPosition", _camera.Position);
            
            // if (Time.time % 1 > 0.5f)
            // {
            //     return;
            // }
            
            float tileHeight = 0.01f / _camera.TargetViewDistance;
            BDebug.TrackFloat(this, "tileHeight", tileHeight);

            if (tileHeight < 1f / Screen.height)
            {
                return;
            }

            int yTileCount = BMath.CeilToInt(1f / tileHeight);
            int xTileCount = BMath.CeilToInt(1f / tileHeight * _screen.Aspect * 0.5f + 1);
            
            int2 gridSize = new (xTileCount, yTileCount);
            
            int tileCount = TileCountFromGridSize(gridSize);
            
            BDebug.TrackInt(this, "tileCount", tileCount);
            
            RenderTexture solverTexture = CreateSolverTexture(gridSize);
            
            RenderTileGrid(gridSize, solverTexture, tileHeight);

            BeastCG.DisposeRenderTexture(solverTexture);
        }

        private RenderTexture CreateSolverTexture(int2 gridSize)
        {
            float screenScale = (float)_terrainDetail.RenderTexture.height / _screen.Height;
            //float2 screenScale = _screen.Size / _terrainDetail.RenderTexture.SizeFloat();
            int tileCount = TileCountFromGridSize(gridSize);
            
            BDebug.TrackFloat(this, "screenScale", screenScale);

            
            int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
            _solverMaterial.SetVector("_TileGridSize", gridSize);
            _solverMaterial.SetTexture("_TerrainTexture", _terrainDetail.RenderTexture);
            
            BDebug.TrackFloat(this, "TargetViewDistance", _camera.TargetViewDistance);

            float2 terrainOldSize = _terrainDetail.RenderTexture.SizeFloat();
            float rSize = _camera.TargetViewDistance / terrainOldSize.y * 100;

            float2 terrainNewSize = terrainOldSize * rSize * screenScale;

            BDebug.TrackFloat(this, "rSize", rSize);
            
            _solverMaterial.SetVector("_TerrainSize", terrainNewSize);
            
            BDebug.TrackFloat(this, "sizeRel", terrainNewSize.x / terrainOldSize.x);

            BDebug.TrackFloat2(this, "terrainNewSize", terrainNewSize);
            BDebug.TrackFloat2(this, "terrainOldSize", terrainOldSize);
            
            BDebug.TrackFloat2(this, "gridSize", gridSize);
            
            // Unknown magic here... Works only with square terrain textures.
            // 1 screen height : 1 detailed render height
            float xOffset = terrainNewSize.x * (1f - 0.25f * _screen.Aspect);
            float yOffset = terrainNewSize.y * (1f + 0.25f * _screen.Aspect);
            
            // 1 screen height : 2 detailed render height
            // float xOffset = terrainNewSize.x * (0.75f - 0.125f * _screen.Aspect);
            // float yOffset = terrainNewSize.y * (0.75f + 0.125f * _screen.Aspect);
            //
            // float xOffset = terrainNewSize.x * _library.xOffset;
            // float yOffset = terrainNewSize.y * _library.yOffset;
            
            BDebug.TrackFloat(this, "xOffset", xOffset);
            BDebug.TrackFloat(this, "yOffset", yOffset);
            
            _solverMaterial.SetVector("_TerrainOffset", new float2(xOffset, yOffset));
            
            
            
            _solverMaterial.SetTexture("_NoiseTexture", _library.worldNoiseTexture);
            _solverMaterial.SetVector("_NoiseSize", _library.worldNoiseTexture.SizeFloat());
            // TODO : Not correct.
            _solverMaterial.SetVector("_NoiseOffset", _camera.Position);
            _solverMaterial.SetFloat("_OutputTextureSize", solverTextureSize);
        
            RenderTexture solverTexture = BeastCG.CreateTemporaryRenderTexture(this, new NewTempRenderTextureData
            {
                size = new int2(solverTextureSize, solverTextureSize),
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

        private void RenderTileGrid(int2 gridSize, RenderTexture solverTexture, float tileHeight)
        {
            RenderParams renderParams = new ()
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one),
                material = _renderMaterial,
                matProps = new MaterialPropertyBlock()
            };

            TerrainTileAtlas atlas = _library.terrainTileAtlas;
        
            renderParams.matProps.SetVector("_TileGridSize", gridSize);
            renderParams.matProps.SetTexture("_TileAtlas", atlas.atlasTexture);
            renderParams.matProps.SetVector("_TileAtlasWidthHeight", atlas.atlasTexture.SizeFloat());
            renderParams.matProps.SetVector("_TileAtlasTileCount", atlas.tileCount);
            renderParams.matProps.SetVector("_TileBlendCenter", atlas.tileBlendCenter);
            renderParams.matProps.SetFloat("_TileAtlasPPU", atlas.astlasPPU);
            renderParams.matProps.SetFloat("_RenderYCrop", atlas.renderYCrop);
            renderParams.matProps.SetTexture("_SolverTexture", solverTexture);
            renderParams.matProps.SetFloat("_SolverSize", solverTexture.width);
            renderParams.matProps.SetVector("_OutputSize", _screen.Size);
            // TODO : Not correct.


            float2 screenSize = _screen.Size;
            BDebug.TrackFloat2(this, "screenSize", screenSize);

            BDebug.TrackFloat(this, "screenAspect", _screen.Aspect);
            BDebug.TrackFloat(this, "screenAspectInv", 1 / _screen.Aspect);
            
            float2 outputOffset = new float2(0, 0);
            BDebug.TrackFloat2(this, "outputOffset", outputOffset);
            
            renderParams.matProps.SetVector("_OutputOffset", outputOffset);
            renderParams.matProps.SetFloat("_OutputTileHeight", tileHeight);
        
            int tileCount = TileCountFromGridSize(gridSize);
            
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