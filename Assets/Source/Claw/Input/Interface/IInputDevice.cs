using Beast.Claw.Common;
using Unity.Collections;

namespace Beast.Claw.Input
{
    public interface IInputDevice : IController
    {
        public DragAxis Drag { get; }
        public ZoomAxis Zoom { get; }
        public NativeSlice<RawTouchAxis> RawTouch { get; }
    }
}