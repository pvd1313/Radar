using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class DeviceScreen : IScreen, IGameCycleListener
    {
        public int2 Size => new(Screen.width, Screen.height);
        
        public float Aspect => Screen.width / (float)Screen.height;
        
        public int Width => Screen.width;
        
        public int Height => Screen.height;
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenUpdate(ActionID.TICK, _tick);
        }

        private void _tick(BlockingContext context)
        {
            BDebug.Track(this, nameof(Size), Size);
            BDebug.Track(this, nameof(Aspect), Aspect);
        }
    }
}