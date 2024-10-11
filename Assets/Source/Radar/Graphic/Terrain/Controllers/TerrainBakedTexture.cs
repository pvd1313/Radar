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
        public RenderTexture Texture { get; private set; }
        public bool IsReady { get; private set; }

        private readonly GameModel _game;
        private readonly ITerrainBaker _baker;
        
        public TerrainBakedTexture(GameModel game, ITerrainBaker baker)
        {
            _game = game;
            _baker = baker;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.GameLoading, GameLoading);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private async UniTask<OperationResult> GameLoading()
        {
            await Dispose();

            Texture = _baker.Create(this, _game.terrain.size, false);

            _baker.Bake(_game.terrain, Texture);

            IsReady = true;

            return OperationResult.Success();
        }

        private UniTask<OperationResult> Dispose()
        {
            IsReady = false;
            
            if (Texture != null)
            {
                BeastCG.DisposeRenderTexture(Texture);
                Texture = null;
            }

            return OperationResult.SuccessSync();
        }
    }
}