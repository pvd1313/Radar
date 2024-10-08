using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;

namespace Beast.Radar.GameCycle
{
    public class GameCycle : Zenject.IInitializable
    {
        private readonly IGameCycle _gameCycle;

        private GameUpdateHandler _mono;

        public GameCycle(IGameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }

        public async void Initialize()
        {
            bool success = _gameCycle.TryInitializeSelf(new GameCycleParameters
            {
                initializationEventIDs = new HashSet<string>
                {
                    OperationID.Boot,
                    OperationID.Warmup,
                    OperationID.GameSaving,
                    OperationID.GameLoading,
                    OperationID.Dispose
                },

                updateEventIDs = new HashSet<string>
                {
                    UpdateID.FrameBegin,
                    UpdateID.FrameEnd,
                    UpdateID.FixedUpdate,
                    UpdateID.GizmosDraw
                }
            });

            if (!success)
            {
                return;
            }

            _mono = BeastScene.CreateGameObject(this).AddComponent<GameUpdateHandler>();
            _mono.FixedUpdated += () => _gameCycle.Update(this, UpdateID.FixedUpdate);
            _mono.GizmosDraw += () => _gameCycle.Update(this, UpdateID.GizmosDraw);
            _mono.FrameBegin += () => _gameCycle.Update(this, UpdateID.FrameBegin);
            _mono.FrameEnd += () => _gameCycle.Update(this, UpdateID.FrameEnd);
            _mono.Destroyed += () => _gameCycle.Execute(this, OperationID.Dispose);
            
            OperationResult result = await _gameCycle.Execute(this, OperationID.Warmup);
            if (result.status != EOperationStatus.Success)
            {
                return;
            }

            await _gameCycle.Execute(this, OperationID.Boot);
        }
    }
}