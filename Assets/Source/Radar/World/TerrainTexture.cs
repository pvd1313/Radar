using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class TerrainTexture : ITerrainTexture, IGameCycleListener
    {
        public RenderTexture Texture { get; private set; }
        public bool IsReady { get; private set; }

        private readonly GameModel _game;
        private readonly ITerrainBaker _baker;
        
        public TerrainTexture(GameModel game, ITerrainBaker baker)
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

            Texture = _baker.CreateTexture(this, _game.terrain.size, false);

            _baker.BakeTexture(new TerrainBakerInput
            {
                biomeScale99 = _game.terrain.biomeScale99,
                waterFlood01 = _game.terrain.waterFlood01,
                seed01 = _game.seed,
                targetTexture = Texture
            });

            IsReady = true;

            return OperationResult.Success();
        }

        private UniTask<OperationResult> Dispose()
        {
            IsReady = false;
            
            if (Texture != null)
            {
                BeastCG.ReleaseRenderTexture(Texture);
                Texture = null;
            }

            return OperationResult.SuccessSync();
        }
    }
}