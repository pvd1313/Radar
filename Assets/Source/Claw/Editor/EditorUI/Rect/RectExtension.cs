using UnityEngine;

namespace Beast.Claw.Editor
{
    public static class RectExtension
    {
        public static Rect2 SliceToColumns2(this Rect screen, float width0, float width1)
        {
            return new Rect2
            {
                rect0 = new Rect(screen.x, screen.y, width0, screen.height),
                rect1 = new Rect(screen.x + width0, screen.y, width1, screen.height),
            };
        }
        
        public static Rect3 SliceToColumns3(this Rect screen, float width0, float width1, float width2)
        {
            return new Rect3
            {
                rect0 = new Rect(screen.x, screen.y, width0, screen.height),
                rect1 = new Rect(screen.x + width0, screen.y, width1, screen.height),
                rect2 = new Rect(screen.x + width0 + width1, screen.y, width2, screen.height),
            };
        }
        
        public static Rect4 SliceToColumns4(this Rect screen, float width0, float width1, float width2, float width3)
        {
            return new Rect4
            {
                rect0 = new Rect(screen.x, screen.y, width0, screen.height),
                rect1 = new Rect(screen.x + width0, screen.y, width1, screen.height),
                rect2 = new Rect(screen.x + width0 + width1, screen.y, width2, screen.height),
                rect3 = new Rect(screen.x + width0 + width1 + width2, screen.y, width3, screen.height)
            };
        }

        public static Rect2 SliceToRows2(this Rect screen, float height0, float height1)
        {
            return new Rect2
            {
                rect0 = new Rect(screen.x, screen.y, screen.width, height0),
                rect1 = new Rect(screen.x, screen.y + height0, screen.width, height1)
            };
        }
        
        public static Rect3 SliceToRows3(this Rect screen, float height0, float height1, float height2)
        {
            return new Rect3
            {
                rect0 = new Rect(screen.x, screen.y, screen.width, height0),
                rect1 = new Rect(screen.x, screen.y + height0, screen.width, height1),
                rect2 = new Rect(screen.x, screen.y + height0 + height1, screen.width, height2)
            };
        }
        
        public static Rect4 SliceToRows4(this Rect screen, float height0, float height1, float height2, float height3)
        {
            return new Rect4
            {
                rect0 = new Rect(screen.x, screen.y, screen.width, height0),
                rect1 = new Rect(screen.x, screen.y + height0, screen.width, height1),
                rect2 = new Rect(screen.x, screen.y + height0 + height1, screen.width, height2),
                rect3 = new Rect(screen.x, screen.y + height0 + height1 + height2, screen.width, height3)
            };
        }

        public static Rect Padding(this Rect rect, Padding padding)
        {
            Vector2 position = rect.position;
            position.x += padding.left;
            position.y += padding.top;

            Vector2 size = rect.size;
            size.x -= padding.right + padding.left;
            size.y -= padding.bottom + padding.top;

            return new Rect(position, size);
        }
    }
}