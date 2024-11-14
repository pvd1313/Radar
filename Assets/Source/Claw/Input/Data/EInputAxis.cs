using System;

namespace Beast.Claw.Input
{
    [Flags]
    public enum EInputAxis
    {
        None = 0,
        RawTouch = 1,
        Drag = 2,
        Zoom = 4
    }
}