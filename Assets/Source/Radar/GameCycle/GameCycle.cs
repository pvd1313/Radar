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
                    UpdateID.Tick,
                    UpdateID.Render,
                    UpdateID.FixedUpdate,
                    UpdateID.GizmosDraw
                }
            });

            if (!success)
            {
                return;
            }
            
            OperationResult result = await _gameCycle.Execute(this, OperationID.Warmup);
            if (result.IsFailed())
            {
                return;
            }

            result = await _gameCycle.Execute(this, OperationID.Boot);
            if (result.IsFailed())
            {
                return;
            }
            
            _mono = BeastScene.CreateGameObject(this).AddComponent<GameUpdateHandler>();
            _mono.FixedUpdated += () => _gameCycle.Update(this, UpdateID.FixedUpdate);
            _mono.GizmosDraw += () => _gameCycle.Update(this, UpdateID.GizmosDraw);
            _mono.FrameBegin += () => _gameCycle.Update(this, UpdateID.Tick);
            _mono.FrameEnd += () => _gameCycle.Update(this, UpdateID.Render);
            _mono.Destroyed += () => _gameCycle.Execute(this, OperationID.Dispose);
        }
    }
}