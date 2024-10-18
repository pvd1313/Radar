using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBakedTexture : ITerrainBakedTexture, IGameCycleListener
    {
        public RenderTexture Texture => _texture;
        public bool IsReady { get; private set; }

        private readonly GameModel _game;
        private readonly ITerrainBaker _baker;
        private readonly RenderConfig _config;
        private RenderTexture _texture;
        
        public TerrainBakedTexture(
            GameModel game, 
            ITerrainBaker baker, 
            RenderConfig config)
        {
            _game = game;
            _baker = baker;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.GameLoad, _gameLoading);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
        }
        
        private async UniTask<OperationResult> _gameLoading()
        {
            await _dispose();
            
            RenderTextureData textureData = _config.terrain.bakedTexture.WithSize(_game.terrain.size);
            _texture = BGraphic.CreatePermRenderTexture(this, textureData);

            _baker.Render(new TerrainBakeTask
            {
                terrain = _game.terrain,
                outputTexture = _texture
            });

            IsReady = true;

            return OperationResult.Success();
        }

        private UniTask<OperationResult> _dispose()
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);

            return OperationResult.SuccessSync();
        }
    }
}