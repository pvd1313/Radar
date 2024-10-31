using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedFbm : Controller, ITerrainBakedFbm, IGameCycleListener
    {
        public bool IsReady { get; private set; }
        public Texture Texture => _texture;

        private readonly GameModel _game;
        private readonly ITerrainFbmRender _fbmRender;
        private readonly GraphicConfig _config;
        private Texture2D _texture;
        private TerrainData _terrainData;
        
        public TerrainBakedFbm(
            GameModel game, 
            ITerrainFbmRender fbmRender, 
            GraphicConfig config)
        {
            _game = game;
            _fbmRender = fbmRender;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            if (_terrainData.IsSame(_game.terrain))
            {
                IsReady = true;
                return UniTask.CompletedTask;
            }
            
            _dispose(default);

            RenderTextureData tempData = _config.terrain.fbmRenderTexture.WithSize(_game.terrain.terrainSize);
            
            Profiler.BeginSample("CreateTempRenderTexture");
            RenderTexture renderTexture = BGraphic.CreateTempRenderTexture(this, tempData);
            Profiler.EndSample();
            
            Profiler.BeginSample("fbmRender.Render");
            _fbmRender.Render(new TerrainFbmRenderTask
            {
                terrain = _game.terrain,
                outputTexture = renderTexture,
                quality01 = 1
            });
            Profiler.EndSample();

            Texture2DParameters permData = _config.terrain.fbmTexture2D.WithSize(_game.terrain.terrainSize);
            
            Profiler.BeginSample("CreateTexture2D");
            _texture = BGraphic.CreateTexture2D(this, permData);
            Profiler.EndSample();

            Profiler.BeginSample("BlitRenderTextureTo2D");
            BGraphic.BlitRenderTextureTo2D(renderTexture, _texture);
            Profiler.EndSample();
            
            BGraphic.Dispose(renderTexture);

            _terrainData = _game.terrain;

            IsReady = true;

            return UniTask.CompletedTask;
        }

        private void _dispose(BlockingContext context)
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);
        }
    }
}