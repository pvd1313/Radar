using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;

namespace Beast.Radar
{
    public class RadarGameCycle : Zenject.IInitializable
    {
        private readonly IBeastGameCycle _gameCycle;

        private BeastGameUpdateHandlerMono _mono;

        public RadarGameCycle(IBeastGameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }

        public async void Initialize()
        {
            bool success = _gameCycle.TryInitializeSelf(new BeastGameCycleParameters
            {
                initializationEventIDs = new HashSet<string>
                {
                    BeastCycleEventID.Boot,
                    BeastCycleEventID.Warmup,
                    BeastCycleEventID.GameSaving,
                    BeastCycleEventID.GameLoading,
                    BeastCycleEventID.Dispose
                },

                updateEventIDs = new HashSet<string>
                {
                    BeastCycleEventID.FrameBegin,
                    BeastCycleEventID.FrameEnd,
                    BeastCycleEventID.FixedUpdate,
                    BeastCycleEventID.GizmosDraw
                }
            });

            if (!success)
            {
                return;
            }

            _mono = BeastScene.CreateGameObject(this).AddComponent<BeastGameUpdateHandlerMono>();
            _mono.FixedUpdated += () => _gameCycle.Update(this, BeastCycleEventID.FixedUpdate);
            _mono.GizmosDraw += () => _gameCycle.Update(this, BeastCycleEventID.GizmosDraw);
            _mono.FrameBegin += () => _gameCycle.Update(this, BeastCycleEventID.FrameBegin);
            _mono.FrameEnd += () => _gameCycle.Update(this, BeastCycleEventID.FrameEnd);
            _mono.Destroyed += () => _gameCycle.Execute(this, BeastCycleEventID.Dispose);
            
            BeastInitializationResult result = await _gameCycle.Execute(this, BeastCycleEventID.Warmup);
            if (result.status != EBeastInitializationStatus.Success)
            {
                return;
            }

            await _gameCycle.Execute(this, BeastCycleEventID.Boot);
        }
    }
}