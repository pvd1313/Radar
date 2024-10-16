using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.GameCycle
{
    public class GameCycle : Zenject.IInitializable
    {
        private readonly IGameCycle _gameCycle;
        private readonly IGameBoot _boot;

        private GameUpdateHandler _mono;

        public GameCycle(IGameCycle gameCycle, IGameBoot boot)
        {
            _gameCycle = gameCycle;
            _boot = boot;
        }

        public async void Initialize()
        {
            bool success = _gameCycle.TryInitializeSelf(new GameCycleParameters
            {
                initializationEventIDs = new HashSet<string>
                {
                    OperationID.Warmup,
                    OperationID.Dispose,
                    
                    OperationID.GameSave,
                    OperationID.GameLoad,
                    
                    OperationID.PreferencesSave,
                    OperationID.PreferencesLoad
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
            
            OperationResult result = await _gameCycle.Execute(this, OperationID.PreferencesLoad);
            if (result.IsFailed())
            {
                return;
            }
            
            result = await _gameCycle.Execute(this, OperationID.Warmup);
            if (result.IsFailed())
            {
                return;
            }

            result = await _boot.Boot();
            if (result.IsFailed())
            {
                return;
            }
            
            _mono = BScene.CreateGameObject(this).AddComponent<GameUpdateHandler>();
            _mono.FixedUpdated += () => _gameCycle.Update(this, UpdateID.FixedUpdate);
            _mono.GizmosDraw += () => _gameCycle.Update(this, UpdateID.GizmosDraw);
            _mono.FrameBegin += () => _gameCycle.Update(this, UpdateID.Tick);
            _mono.FrameEnd += () => _gameCycle.Update(this, UpdateID.Render);
            _mono.Destroyed += () => UniTask.Create(OnMonoDestroyed);
        }

        // In future we will have to also save preferences on application lost focus.
        private async UniTask OnMonoDestroyed()
        {
            await _gameCycle.Execute(this, OperationID.PreferencesSave);
            await _gameCycle.Execute(this, OperationID.Dispose);
        }
    }
}