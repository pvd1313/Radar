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
            // bool success = _gameCycle.TryInitializeSelf(new GameCycleData
            // {
            //     operationEvents = new HashSet<string>
            //     {
            //         OperationID.WARMUP,
            //         OperationID.DISPOSE,
            //         
            //         OperationID.GAME_SAVE,
            //         OperationID.GAME_LOAD,
            //         
            //         OperationID.PREFERENCES_SAVE,
            //         OperationID.PREFERENCES_LOAD
            //     },
            //
            //     updateEvents = new HashSet<string>
            //     {
            //         UpdateID.TICK,
            //         UpdateID.RENDER,
            //         UpdateID.FIXED_UPDATE,
            //         UpdateID.GIZMOS_DRAW
            //     }
            // });
            //
            // if (!success)
            // {
            //     return;
            // }
            //
            // OperationResult result = await _gameCycle.Execute(this, OperationID.PREFERENCES_LOAD);
            // if (result.IsFailed())
            // {
            //     return;
            // }
            //
            // result = await _gameCycle.Execute(this, OperationID.WARMUP);
            // if (result.IsFailed())
            // {
            //     return;
            // }
            //
            // result = await _boot.Boot();
            // if (result.IsFailed())
            // {
            //     return;
            // }
            //
            // _mono = BScene.CreateGameObject(this).AddComponent<GameUpdateHandler>();
            // _mono.FixedUpdated += () => _gameCycle.Update(this, UpdateID.FIXED_UPDATE);
            // _mono.GizmosDraw += () => _gameCycle.Update(this, UpdateID.GIZMOS_DRAW);
            // _mono.FrameBegin += () => _gameCycle.Update(this, UpdateID.TICK);
            // _mono.FrameEnd += () => _gameCycle.Update(this, UpdateID.RENDER);
            // _mono.Destroyed += () => UniTask.Create(OnMonoDestroyed);
        }

        // In future we will have to also save preferences on application lost focus.
        private async UniTask OnMonoDestroyed()
        {
            await _gameCycle.Execute(this, OperationID.PREFERENCES_SAVE);
            
            await _gameCycle.Execute(this, OperationID.DISPOSE);
            
            _gameCycle.Dispose();
        }
    }
}