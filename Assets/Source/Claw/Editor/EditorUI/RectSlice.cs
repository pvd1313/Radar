using UnityEngine;

namespace Beast.Claw.Editor
{
    public static class RectSlice
    {
        public static (Rect, Rect, Rect, Rect) Horizontal
            (Rect rect, float width0, float width1, float width2, float width3)
        {
            return 
            (
                new Rect(rect.x, rect.y, width0, rect.height),
                new Rect(rect.x + width0, rect.y, width1, rect.height),
                new Rect(rect.x + width0 + width1, rect.y, width2, rect.height),
                new Rect(rect.x + width0 + width1 + width2, rect.y, width3, rect.height)
            );
        }
    }
}