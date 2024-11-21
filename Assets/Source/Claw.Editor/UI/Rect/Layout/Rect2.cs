using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public struct Rect2
    {
        public Rect rect0;
        public Rect rect1;

        public Rect2(Rect r0, Rect r1)
        {
            rect0 = r0;
            rect1 = r1;
        }

        public void Deconstruct(out Rect r0, out Rect r1)
        {
            r0 = rect0;
            r1 = rect1;
        }
    }
}