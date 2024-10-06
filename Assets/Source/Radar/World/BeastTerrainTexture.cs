using Beast.Claw.CG;
using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class BeastTerrainTexture : IBeastTerrainTexture, IBeastGameCycleListener
    {
        public RenderTexture Texture { get; private set; }
        public bool IsReady { get; private set; }

        private readonly BeastGameModel _game;
        private readonly IBeastTerrainBaker _baker;
        
        public BeastTerrainTexture(BeastGameModel game, IBeastTerrainBaker baker)
        {
            _game = game;
            _baker = baker;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.GameLoading, GameLoading);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }
        
        private async UniTask<BeastInitializationResult> GameLoading()
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

            return BeastInitializationResult.Success();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            IsReady = false;
            
            if (Texture != null)
            {
                BeastCG.ReleaseRenderTexture(Texture);
                Texture = null;
            }

            return BeastInitializationResult.SuccessSync();
        }
    }
}