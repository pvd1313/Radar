using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedBiome : Controller, ITerrainBackedBiome, IGameCycleListener
    {
        private const int MAP_1KM_BLOCK_SIZE = 128;

        public TextureWithBlocks Terrain1kmCell => _biomeWithBlocks;
        public AveragePixelCountTexture Terrain8kmCell => _biomePacked;
        
        public bool IsReady { get; private set; }

        private readonly ITerrainBakedFbm _bakedTerrain;
        private readonly IRender<TerrainBiomeRenderTask> _biomeRender;
        private readonly GraphicConfig _graphicConfig;
        private readonly GameModel _game;
        private TextureWithBlocks _biomeWithBlocks;
        private AveragePixelCountTexture _biomePacked;
        private TerrainData _bakedForTerrain;

        public TerrainBakedBiome(
            ITerrainBakedFbm bakedTerrain, 
            IRender<TerrainBiomeRenderTask> biomeRender,
            GraphicConfig graphicConfig,
            GameModel game)
        {
            _bakedTerrain = bakedTerrain;
            _biomeRender = biomeRender;
            _graphicConfig = graphicConfig;
            _game = game;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            if (_bakedForTerrain.IsSame(_game.terrain))
            {
                IsReady = true;
                
                return UniTask.CompletedTask;
            }
            
            _dispose(default);
            
            Application.targetFrameRate = 60;
            
            Profiler.BeginSample("CreateTempRenderTexture");
            RenderTextureData renderTextureData = _graphicConfig.terrain.biomeRenderTexture.WithSize(_game.terrain.terrainSize);
            RenderTexture biomeRenderTexture = BGraphic.CreateTempRenderTexture(this, renderTextureData);
            Profiler.EndSample();
            
            Profiler.BeginSample("_biomeRender.Render");
            _biomeRender.Render(new TerrainBiomeRenderTask
            {
                screenSize = _game.terrain.terrainSize,
                useDynamicConstraints = false,
                renderPosition = _game.terrain.terrainSize.Multiply(0.5f),
                bakedTerrain = _bakedTerrain.Texture,
                viewDistance = _game.terrain.terrainSize.y,
                outputBiomeTexture = biomeRenderTexture
            });
            Profiler.EndSample();

            Profiler.BeginSample("CreateTexture2D");
            Texture2DParameters textureParams = _graphicConfig.terrain.biomeTexture2D.WithSize(_game.terrain.terrainSize);
            Texture2D biomeTexture2D = BGraphic.CreateTexture2D(this, textureParams);
            BGraphic.BlitRenderTextureTo2D(biomeRenderTexture, biomeTexture2D);
            Profiler.EndSample();

            Profiler.BeginSample("TextureWithBlocks.FromBiomeTexture");
            _biomeWithBlocks = TextureWithBlocks.FromBiomeTexture(biomeTexture2D, MAP_1KM_BLOCK_SIZE);
            Profiler.EndSample();

            Profiler.BeginSample("AveragePixelCountTexture.FromTextureWithBlocks()");
            int cellSize = BMath.Clamp(_game.terrain.terrainSize.x / 256, 8, 16);
            _biomePacked = AveragePixelCountTexture.FromTextureWithBlocks(_biomeWithBlocks, cellSize);
            Profiler.EndSample();
            
            Profiler.BeginSample("Dispose Textures");
            BGraphic.Dispose(biomeRenderTexture);
            BGraphic.Dispose(biomeTexture2D);
            Profiler.EndSample();

            IsReady = true;
            
            return UniTask.CompletedTask;
        }
        
        private void _dispose(BlockingContext obj)
        {
            IsReady = false;
            
            _biomeWithBlocks.Dispose();
            _biomePacked.Dispose();
        }
    }
}