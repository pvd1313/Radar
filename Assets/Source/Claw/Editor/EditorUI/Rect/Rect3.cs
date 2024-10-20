using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Rect3
    {
        public Rect rect0;
        public Rect rect1;
        public Rect rect2;
        
        public void Deconstruct(out Rect r0, out Rect r1, out Rect r2)
        {
            r0 = rect0;
            r1 = rect1;
            r2 = rect2;
        }
    }
}