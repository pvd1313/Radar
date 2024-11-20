using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public struct Rect4
    {
        public Rect rect0;
        public Rect rect1;
        public Rect rect2;
        public Rect rect3;
        
        public Rect4(Rect r0, Rect r1, Rect r2, Rect r3)
        {
            rect0 = r0;
            rect1 = r1;
            rect2 = r2;
            rect3 = r3;
        }
        
        public void Deconstruct(out Rect r0, out Rect r1, out Rect r2, out Rect r3)
        {
            r0 = rect0;
            r1 = rect1;
            r2 = rect2;
            r3 = rect3;
        }
    }
}