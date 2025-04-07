using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedBiome : Actor, ITerrainBackedBiome
    {
        private const int MAP_1KM_BLOCK_SIZE = 128;

        public TextureWithBlocks<TerrainCell> Terrain1kmCell => _biomeWithBlocks;
        public AveragePixelCountTexture<TerrainCell> Terrain8kmCell => _biomePacked;
        
        public bool IsReady { get; private set; }

        [Required] private readonly ITerrainBakedFbm _bakedTerrain;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly IRender<TerrainBiomeRenderTask> _biomeRender;
        [Required] private readonly TerrainRenderConfig _config;
        
        private TextureWithBlocks<TerrainCell> _biomeWithBlocks;
        private AveragePixelCountTexture<TerrainCell> _biomePacked;
        private TerrainData _bakedForTerrain;

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;
            
            if (_bakedForTerrain.IsSame(terrain))
            {
                IsReady = true;
                
                return UniTask.CompletedTask;
            }
            
            _dispose(default);
            
            BProfiler.BeginSample("CreateTempRenderTexture");
            RenderTextureData renderTextureData = _config.biomeRenderTexture.WithSize(terrain.terrainSize);
            RenderTexture biomeRenderTexture = BGraphic.CreateTempRenderTexture(this, renderTextureData);
            BProfiler.EndSample();
            
            BProfiler.BeginSample("_biomeRender.Render");
            _biomeRender.Render(new TerrainBiomeRenderTask
            {
                screenSize = terrain.terrainSize,
                useDynamicConstraints = false,
                renderPosition = terrain.terrainSize * 0.5f,
                bakedTerrain = _bakedTerrain.Texture,
                viewDistance = terrain.terrainSize,
                outputBiomeTexture = biomeRenderTexture
            });
            BProfiler.EndSample();

            BProfiler.BeginSample("CreateTexture2D");
            Texture2DParameters textureParams = _config.biomeTexture2D.WithSize(terrain.terrainSize);
            Texture2D biomeTexture2D = BGraphic.CreateTexture2D(this, textureParams);
            BGraphic.BlitRenderTextureTo2D(biomeRenderTexture, biomeTexture2D);
            BProfiler.EndSample();

            BProfiler.BeginSample("TextureWithBlocks.FromBiomeTexture");
            _biomeWithBlocks = TextureWithBlocks<TerrainCell>.FromBiomeTexture(biomeTexture2D, MAP_1KM_BLOCK_SIZE);
            BProfiler.EndSample();

            BProfiler.BeginSample("AveragePixelCountTexture.FromTextureWithBlocks()");
            int cellSize = BMath.Clamp(terrain.terrainSize / 256, 8, 16);
            _biomePacked = AveragePixelCountTexture<TerrainCell>.FromTextureWithBlocks(_biomeWithBlocks, cellSize);
            BProfiler.EndSample();
            
            BProfiler.BeginSample("Dispose Textures");
            BGraphic.Dispose(biomeRenderTexture);
            BGraphic.Dispose(biomeTexture2D);
            BProfiler.EndSample();

            _bakedForTerrain = terrain;

            IsReady = true;
            
            return UniTask.CompletedTask;
        }

        protected override void _dispose(BlockingContext obj)
        {
            IsReady = false;
            
            _biomeWithBlocks.Dispose();
            _biomePacked.Dispose();
        }
    }
}