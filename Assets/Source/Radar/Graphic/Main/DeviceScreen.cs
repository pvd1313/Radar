using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.GameCycle;
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
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenUpdate(this, UpdateID.FrameBegin, FrameBegin);
        }

        private void FrameBegin()
        {
            BDebug.Track(this, "size", Size);
            BDebug.Track(this, "aspect", Aspect);
        }
    }
}