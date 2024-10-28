using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using UnityEngine;

namespace Beast.Radar.GameTime
{
    public class WorldTime : IWorldTime, IGameCycleListener
    {
        private readonly WorldTimeConfig _timeConfig;
        public float Minutes { get; private set; }

        private float _seconds;

        public WorldTime(WorldTimeConfig timeConfig)
        {
            _timeConfig = timeConfig;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenUpdate(ActionID.TICK, _tick);
        }

        private void _tick(ActionContext obj)
        {
            _seconds += Time.deltaTime * _timeConfig.secondMultiplier;

            if (_seconds >= 60)
            {
                Minutes++;
                _seconds -= 60;
                
                BDebug.Track(this, "hour", GetHours());
            }
        }

        public float GetHours()
        {
            return Minutes / 60;
        }
    }
}