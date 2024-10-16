using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainBakedTexture : ITerrainBakedTexture, IGameCycleListener
    {
        public RenderTexture Texture => _texture;
        public bool IsReady { get; private set; }

        private readonly GameModel _game;
        private readonly ITerrainBaker _baker;
        private RenderTexture _texture;
        
        public TerrainBakedTexture(GameModel game, ITerrainBaker baker)
        {
            _game = game;
            _baker = baker;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.GameLoad, GameLoading);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private async UniTask<OperationResult> GameLoading()
        {
            await Dispose();

            _texture = _baker.Create(this, _game.terrain.size, false);

            _baker.Bake(_game.terrain, _texture);

            IsReady = true;

            return OperationResult.Success();
        }

        private UniTask<OperationResult> Dispose()
        {
            IsReady = false;

            BGraphic.TryDispose(ref _texture);

            return OperationResult.SuccessSync();
        }
    }
}