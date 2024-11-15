using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class DeviceScreen : Controller, IScreen, IGameLoopListener
    {
        public int2 Size => new(Screen.width, Screen.height);
        
        public float Aspect => Screen.width / (float)Screen.height;
        
        public int Width => Screen.width;
        
        public int Height => Screen.height;
        
        public void Register(IGameLoopHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private void _tick(BlockingContext context)
        {
            BDebug.Track(this, nameof(Size), Size);
            BDebug.Track(this, nameof(Aspect), Aspect);
        }
    }
}