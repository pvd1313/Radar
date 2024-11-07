using Beast.Claw.Common;
using System;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.GameTime
{
    public class GameTimeController : Controller, IGameTime, IGameCycleListener
    {
        public event Action Changed;
        
        private readonly GameTimeConfig _config;
        private readonly GameModel _gameModel;

        private float _playerStateMultiplier;

        private TimeSpan _gameTimeSpan;
        private bool _isPause;

        public GameTimeController(GameTimeConfig config, GameModel gameModel)
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
            _gameTimeSpan = TimeSpan.FromMinutes(_gameModel.timeMinutes);
            return UniTask.CompletedTask;
        }

        private void _gameSave(BlockingContext context)
        {
            _gameModel.timeMinutes = (ulong)BMath.Floor(_gameTimeSpan.TotalMinutes);
        }

        private void _tick(BlockingContext context)
        {
            if (_isPause)
            {
                return;
            }
            
            _gameTimeSpan += TimeSpan.FromSeconds(Time.deltaTime * _config.secondMultiplier * _playerStateMultiplier);

            if (_gameTimeSpan.TotalSeconds >= 60)
            {
                Changed?.Invoke();
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