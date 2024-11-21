using Beast.Claw.Collections;

namespace Beast.Claw.Input
{
    public interface IInputDriver
    {
        void ReadTouchScreen(ref NativeFixedList<RawTouchAxis> rawTouches);
        RawTouchAxis ReadMousePointer();
        float ReadMouseWheel();
    }
}