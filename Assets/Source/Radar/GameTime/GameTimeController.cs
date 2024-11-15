using Beast.Claw.Common;
using System;
using Beast.Claw.GameLoop;
using Beast.Claw.Math;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.GameTime
{
    public class GameTimeController : Actor, IGameTime
    {
        public event Action MinuteChanged;
        
        private readonly GameTimeConfig _config;
        private readonly GameModel _gameModel;

        private float _playerStateMultiplier;

        private TimeSpan _gameTimeSpan;
        private bool _isPause;
        private int _lastMinute;

        public GameTimeController(GameTimeConfig config, GameModel gameModel)
        {
            _config = config;
            _gameModel = gameModel;
            _playerStateMultiplier = _config.idleTimeScale;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            _gameTimeSpan = TimeSpan.FromMinutes(_gameModel.timeMinutes);
            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext context)
        {
            _gameModel.timeMinutes = (ulong)BMath.Floor(_gameTimeSpan.TotalMinutes);
        }

        protected override void _tick(BlockingContext context)
        {
            if (_isPause)
            {
                return;
            }
            
            _gameTimeSpan += TimeSpan.FromSeconds(Time.deltaTime * _config.secondMultiplier * _playerStateMultiplier);
            int currentMinute = (int)_gameTimeSpan.TotalMinutes;

            if (currentMinute != _lastMinute)
            {
                _lastMinute = currentMinute;
                MinuteChanged?.Invoke();
            }
        }

        public void SetPlayerStateMultiplier(bool onAction)
        {
            _playerStateMultiplier = onAction ? 1 : _config.idleTimeScale;
        }

        public TimeSpan GetTime()
        {
            return _gameTimeSpan;
        }

        public void Start()
        {
            _isPause = false;
        }

        public void Stop()
        {
            _isPause = true;
        }
    }
}