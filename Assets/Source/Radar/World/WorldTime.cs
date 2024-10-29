using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldTime : IWorldTime, IGameCycleListener
    {
        private readonly WorldTimeConfig _config;
        private readonly GameModel _gameModel;

        private ulong _minutes;
        private float _seconds;

        public WorldTime(WorldTimeConfig config, GameModel gameModel)
        {
            _config = config;
            _gameModel = gameModel;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.GAME_SAVE, _gameSave);
        }

        private UniTask<ActionResult> _gameLoad(ActionContext arg)
        {
            _minutes = _gameModel.world.minutes;
            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _gameSave(ActionContext arg)
        {
            _gameModel.world.minutes = _minutes;
            return ActionResult.SuccessSyncronous();
        }

        private void _tick(ActionContext obj)
        {
            _seconds += Time.deltaTime * _config.secondMultiplier;

            if (_seconds >= 60)
            {
                _minutes++;
                _seconds -= 60;
                BDebug.Track(this, "time", $"{GetHours()}:{GetMinutes()}");
            }
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