using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldTime : IWorldTime, IGameCycleListener
    {
        private readonly WorldTimeConfig _timeConfig;
        private readonly GameModel _gameModel;

        private ulong _minutes;
        private float _seconds;

        public WorldTime(WorldTimeConfig timeConfig, GameModel gameModel)
        {
            _timeConfig = timeConfig;
            _gameModel = gameModel;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.GAME_LOAD, _gameSave);
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
            _seconds += Time.deltaTime * _timeConfig.secondMultiplier;

            if (_seconds >= 60)
            {
                _minutes++;
                
                _seconds -= 60;
                BDebug.Track(this, "hour", GetHours());
            }
        }

        public float GetHours()
        {
            return (_minutes / 60) % 24;
        }
    }
}