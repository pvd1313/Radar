using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedTexture : ITerrainBakedTexture, IGameCycleListener
    {
        public Texture Texture => _texture;
        public bool IsReady { get; private set; }

        private readonly GameModel _game;
        private readonly ITerrainBaker _baker;
        private readonly GraphicConfig _config;
        private Texture2D _texture;
        
        public TerrainBakedTexture(
            GameModel game, 
            ITerrainBaker baker, 
            GraphicConfig config)
        {
            _game = game;
            _baker = baker;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask<ActionResult> _gameLoad(ConcurrentContext context)
        {
            _dispose(default);

            RenderTextureData tempData = _config.terrain.bakerRenderTexture.WithSize(_game.terrain.terrainSize);
            
            Profiler.BeginSample("TerrainBakedTexture._gameLoading() #0");
            
            RenderTexture renderTexture = BGraphic.CreateTempRenderTexture(this, tempData);
            
            Profiler.EndSample();
            
            Profiler.BeginSample("TerrainBakedTexture._gameLoading() #1");
            
            _baker.Render(new TerrainBakeTask
            {
                terrain = _game.terrain,
                outputTexture = renderTexture,
                quality01 = 1
            });
            
            Profiler.EndSample();

            Texture2DParameters permData = _config.terrain.bakerTexture2D.WithSize(_game.terrain.terrainSize);
            
            Profiler.BeginSample("TerrainBakedTexture._gameLoading() #2");
            _texture = BGraphic.CreateTexture2D(this, permData);
            Profiler.EndSample();

            Profiler.BeginSample("TerrainBakedTexture._gameLoading() #3");
            BGraphic.BlitRenderTextureTo2D(renderTexture, _texture);
            Profiler.EndSample();
            
            BGraphic.Dispose(renderTexture);

            IsReady = true;

            return ActionResult.SuccessSyncronous();
        }

        private void _dispose(BlockingContext context)
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);
        }
    }
}