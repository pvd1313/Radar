using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class DeviceScreen : Actor, IScreen
    {
        public int2 Size => new(Screen.width, Screen.height);
        
        public float Aspect => Screen.width / (float)Screen.height;
        
        public float PPU => Screen.dpi;
        
        protected override void _tick(BlockingContext context)
        {
            BDebug.Track(this, nameof(Size), Size);
            BDebug.Track(this, nameof(Aspect), Aspect);
            BDebug.Track(this, nameof(PPU), PPU);
        }
    }
}