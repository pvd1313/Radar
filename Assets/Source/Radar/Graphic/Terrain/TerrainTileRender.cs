using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.WSA;
using Screen = UnityEngine.Device.Screen;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainTileRender : IGameCycleListener
    {
        private readonly ICameraTransform _camera;
        private readonly GraphicLibrary _library;
        private readonly IScreen _screen;
        private readonly ITerrainNoiseRender _terrainNoise;
        private readonly ITerrainBakedTexture _terrainBaked;

        private Material _renderMaterial;
        private Material _solverMaterial;

        public TerrainTileRender(
            ICameraTransform camera, 
            GraphicLibrary library, 
            IScreen screen,
            ITerrainNoiseRender terrainNoise,
            ITerrainBakedTexture terrainBaked)
        {
            _camera = camera;
            _library = library;
            _screen = screen;
            _terrainNoise = terrainNoise;
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
            float viewDistance = _camera.TargetViewDistance;
            
            float tileHeight = (1f / _camera.TargetViewDistance);
            BDebug.TrackFloat(this, "tileHeight", tileHeight);

            int yTileCount = BMath.CeilToInt(1f / tileHeight + 1);
            int xTileCount = BMath.CeilToInt(1f / tileHeight * _screen.Aspect);
            
            int2 gridSize = new (xTileCount, yTileCount);
            
            int tileCount = TileCountFromGridSize(gridSize);

            if (tileCount > 100000)
            {
                return;
            }
            
            BDebug.TrackInt(this, "tileCount", tileCount);
            
            RenderTexture solverTexture = CreateSolverTexture(gridSize);
            
            RenderTileGrid(gridSize, solverTexture, tileHeight);

            BeastCG.DisposeRenderTexture(solverTexture);
        }

        private RenderTexture CreateSolverTexture(int2 gridSize)
        {
            int tileCount = TileCountFromGridSize(gridSize);
            
            BDebug.TrackFloat2(this, "cameraPosition", _camera.Position);
            
            int solverTextureSize = BMath.CeilToInt(BMath.Sqrt(tileCount));
            _solverMaterial.SetVector("_TileGridSize", gridSize);
            _solverMaterial.SetTexture("_TerrainTexture", _terrainBaked.Texture);
            _solverMaterial.SetVector("_TerrainSize", _terrainBaked.Texture.WidthHeightFloat());
            // TODO : Not correct.
            _solverMaterial.SetVector("_TerrainOffset", _camera.ViewportToWorld(new float2(0, 1)));
            _solverMaterial.SetTexture("_NoiseTexture", _library.worldNoiseTexture);
            _solverMaterial.SetVector("_NoiseSize", _library.worldNoiseTexture.WidthHeightFloat());
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
            renderParams.matProps.SetVector("_TileAtlasWidthHeight", atlas.atlasTexture.WidthHeightFloat());
            renderParams.matProps.SetVector("_TileAtlasTileCount", atlas.tileCount);
            renderParams.matProps.SetVector("_TileBlendCenter", atlas.tileBlendCenter);
            renderParams.matProps.SetFloat("_TileAtlasPPU", atlas.astlasPPU);
            renderParams.matProps.SetFloat("_RenderYCrop", atlas.renderYCrop);
            renderParams.matProps.SetTexture("_SolverTexture", solverTexture);
            renderParams.matProps.SetFloat("_SolverSize", solverTexture.width);
            renderParams.matProps.SetVector("_OutputSize", _screen.Size);
            // TODO : Not correct.
            renderParams.matProps.SetVector("_OutputOffset", new float2(0.0f, 0.0f));
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