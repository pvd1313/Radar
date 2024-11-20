using System.Runtime.CompilerServices;
using Beast.Claw.Collections;

namespace Beast.Claw.Input
{
    public static class RawTouchAxisExtension
    {
        public static bool ContainsWorkingOrStartingTouch(this NativeFixedList<RawTouchAxis> touches)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].status != EAxisStatus.Disabled)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsDisabledTouch(this NativeFixedList<RawTouchAxis> touches)
        {
            return _containsTouchWithStatus(touches, EAxisStatus.Disabled);
        }
        
        public static bool ContainsStartingTouch(this NativeFixedList<RawTouchAxis> touches)
        {
            return _containsTouchWithStatus(touches, EAxisStatus.Starting);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _containsTouchWithStatus(NativeFixedList<RawTouchAxis> touches, EAxisStatus status)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].status == status)
                {
                    return true;
                }
            }

            return false;
        }
    }
}