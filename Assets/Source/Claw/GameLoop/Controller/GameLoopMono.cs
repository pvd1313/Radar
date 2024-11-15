using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.GameLoop
{
    public class GameLoopMono : MonoBehaviour, IController
    {
        public ControllerMeta Meta { get; private set; }
        
        private IGameLoop _gameLoop;
        private bool _savedPreferences;

        private void Awake()
        {
            Meta = ControllerMeta.Create(this);
        }

        public void Initialize(IGameLoop gameLoop)
        {
            _gameLoop = gameLoop;
        }

        private void Update()
        {
            _gameLoop.ExecuteBlocking(this, ActionID.TICK);
        }

        private void FixedUpdate()
        {
            _gameLoop.ExecuteBlocking(this, ActionID.FIXED_UPDATE);
        }

        private void LateUpdate()
        {
            _gameLoop.ExecuteBlocking(this, ActionID.RENDER);
        }

        private void OnDestroy()
        {
            _gameLoop.ExecuteBlocking(this, ActionID.PREFERENCES_SAVE);
            
            _gameLoop.ExecuteBlocking(this, ActionID.DISPOSE);
            
            _gameLoop.Dispose();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                _gameLoop.ExecuteBlocking(this, ActionID.PREFERENCES_SAVE);
            }
        }
    }
}