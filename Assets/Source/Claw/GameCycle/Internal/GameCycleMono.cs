using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.GameCycle
{
    public class GameCycleMono : MonoBehaviour
    {
        private IGameCycle _gameCycle;
        
        public void Initialize(IGameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }

        private void Update()
        {
            _gameCycle.Update(this, ActionID.TICK);
        }

        private void FixedUpdate()
        {
            _gameCycle.Update(this, ActionID.FIXED_UPDATE);
        }

        private void OnDrawGizmos()
        {
            _gameCycle.Update(this, ActionID.GIZMOS_DRAW);
        }

        private void LateUpdate()
        {
            _gameCycle.Update(this, ActionID.RENDER);
        }

        private void OnDestroy()
        {
            UniTask.Create(OnMonoDestroyed);
        }

        private async UniTask OnMonoDestroyed()
        {
            await _gameCycle.Execute(this, ActionID.PREFERENCES_SAVE);
            
            await _gameCycle.Execute(this, ActionID.DISPOSE);
            
            _gameCycle.Dispose();
        }
    }
}