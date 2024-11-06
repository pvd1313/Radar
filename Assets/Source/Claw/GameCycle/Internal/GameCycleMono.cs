using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.GameCycle
{
    public class GameCycleMono : MonoBehaviour, IController
    {
        public ControllerMeta Meta { get; private set; }
        
        private IGameCycle _gameCycle;
        private bool _savedPreferences;

        private void Awake()
        {
            Meta = ControllerMeta.Create(this);
        }

        public void Initialize(IGameCycle gameCycle)
        {
            _gameCycle = gameCycle;
        }

        private void Update()
        {
            _gameCycle.ExecuteBlocking(this, ActionID.TICK);
        }

        private void FixedUpdate()
        {
            _gameCycle.ExecuteBlocking(this, ActionID.FIXED_UPDATE);
        }

        private void LateUpdate()
        {
            _gameCycle.ExecuteBlocking(this, ActionID.RENDER);
        }

        private void OnDestroy()
        {
            _gameCycle.ExecuteBlocking(this, ActionID.PREFERENCES_SAVE);
            
            _gameCycle.ExecuteBlocking(this, ActionID.DISPOSE);
            
            _gameCycle.Dispose();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                _gameCycle.ExecuteBlocking(this, ActionID.PREFERENCES_SAVE);
            }
        }
    }
}