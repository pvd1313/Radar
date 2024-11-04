using Beast.Claw.Common;
using System;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.GameTime
{
    public class GameTimeController : Controller, IGameTime, IGameCycleListener
    {
        public event Action Changed;
        
        private readonly WorldTimeConfig _config;
        private readonly GameTimeConfig _config;
        private readonly GameModel _gameModel;

        private float _playerStateMultiplier;

        private bool _isPause;
        private ulong _minutes;
        private float _seconds;

        public WorldTime(WorldTimeConfig config, GameModel gameModel)
        {
            _config = config;
            _gameModel = gameModel;
            _playerStateMultiplier = _config.idleTimeScale;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
        }

        private UniTask _gameLoad(ConcurrentContext context)
        {
            _minutes = _gameModel.world.minutes;
            return UniTask.CompletedTask;
        }

        private void _gameSave(BlockingContext context)
        {
            _gameModel.world.minutes = _minutes;
        }

        private void _tick(BlockingContext context)
        {
            if (_isPause)
            {
                return;
            }
            
            _seconds += Time.deltaTime * _config.secondMultiplier * _playerStateMultiplier;

            if (_seconds >= 60)
            {
                _minutes++;
                _seconds -= 60;

                Changed?.Invoke();
                
                BDebug.Track(this, "time", $"{GetHours()}:{GetMinutes()}");
            }
        }

        public void SetPlayerStateMultiplier(bool onAction)
        {
            _playerStateMultiplier = onAction ? 1 : _config.idleTimeScale;
        }

        public void Start()
        {
            _isPause = false;
        }

        public void Stop()
        {
            _isPause = true;
        }

        public int GetDays()
        {
            return (int)(_minutes / (60 * 24));
        }

        public float GetHours()
        {
            return (_minutes / 60) % 24;
        }

        public float GetMinutes()
        {
            return _minutes % 60;
        }

        public float GetTotalHours()
        {
            return (float)_minutes / 60 + (_seconds / 3600);
        }

        public string CalculateTravelTime(float2 a, float2 b, float speed)
        {
            float distance = BMath.Distance(a, b);
            float timeInSeconds = distance / speed * 3600 * _config.secondMultiplier;

            int hours = (int)(timeInSeconds / 86400);              
            int minutes = (int)((timeInSeconds % 86400) / 3600);   
            
            return $"{hours:D2}:{minutes:D2}";
        }
    }
}