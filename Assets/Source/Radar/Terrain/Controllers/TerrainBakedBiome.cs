using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedBiome : Actor, ITerrainBackedBiome
    {
        private const int MAP_1KM_BLOCK_SIZE = 128;

        public TextureWithBlocks<EBiomeName> Terrain1kmCell => _biomeWithBlocks;
        public AveragePixelCountTexture<EBiomeName> Terrain8kmCell => _biomePacked;
        
        public bool IsReady { get; private set; }

        [Required] private readonly ITerrainBakedFbm _bakedTerrain;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly IRender<TerrainBiomeRenderTask> _biomeRender;
        [Required] private readonly GraphicConfig _graphicConfig;
        
        private TextureWithBlocks<EBiomeName> _biomeWithBlocks;
        private AveragePixelCountTexture<EBiomeName> _biomePacked;
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
            
            Profiler.BeginSample("CreateTempRenderTexture");
            RenderTextureData renderTextureData = _graphicConfig.terrain.biomeRenderTexture.WithSize(terrain.terrainSize);
            RenderTexture biomeRenderTexture = BGraphic.CreateTempRenderTexture(this, renderTextureData);
            Profiler.EndSample();
            
            Profiler.BeginSample("_biomeRender.Render");
            _biomeRender.Render(new TerrainBiomeRenderTask
            {
                screenSize = terrain.terrainSize,
                useDynamicConstraints = false,
                renderPosition = terrain.terrainSize * 0.5f,
                bakedTerrain = _bakedTerrain.Texture,
                viewDistance = terrain.terrainSize,
                outputBiomeTexture = biomeRenderTexture
            });
            Profiler.EndSample();

            Profiler.BeginSample("CreateTexture2D");
            Texture2DParameters textureParams = _graphicConfig.terrain.biomeTexture2D.WithSize(terrain.terrainSize);
            Texture2D biomeTexture2D = BGraphic.CreateTexture2D(this, textureParams);
            BGraphic.BlitRenderTextureTo2D(biomeRenderTexture, biomeTexture2D);
            Profiler.EndSample();

            Profiler.BeginSample("TextureWithBlocks.FromBiomeTexture");
            _biomeWithBlocks = TextureWithBlocks<EBiomeName>.FromBiomeTexture(biomeTexture2D, MAP_1KM_BLOCK_SIZE);
            Profiler.EndSample();

            Profiler.BeginSample("AveragePixelCountTexture.FromTextureWithBlocks()");
            int cellSize = BMath.Clamp(terrain.terrainSize / 256, 8, 16);
            _biomePacked = AveragePixelCountTexture<EBiomeName>.FromTextureWithBlocks(_biomeWithBlocks, cellSize);
            Profiler.EndSample();
            
            Profiler.BeginSample("Dispose Textures");
            BGraphic.Dispose(biomeRenderTexture);
            BGraphic.Dispose(biomeTexture2D);
            Profiler.EndSample();

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