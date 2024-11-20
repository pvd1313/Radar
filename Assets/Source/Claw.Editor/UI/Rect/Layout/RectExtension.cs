using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public static class RectExtension
    {
        public static Rect2 SliceToColumns(this Rect rect, float w0, float w1)
        {
            RectSlice slice = new(stackalloc Rect[2]);
            
            slice.Columns(rect, w0, w1);

            return new Rect2(slice[0], slice[1]);
        }
        
        public static Rect3 SliceToColumns(this Rect rect, float w0, float w1, float w2)
        {
            RectSlice slice = new(stackalloc Rect[3]);
            
            slice.Columns(rect, w0, w1, w2);
            
            return new Rect3(slice[0], slice[1], slice[2]);
        }
        
        public static Rect4 SliceToColumns(this Rect rect, float w0, float w1, float w2, float w3)
        {
            RectSlice slice = new(stackalloc Rect[4]);
            
            slice.Columns(rect, w0, w1, w2, w3);
            
            return new Rect4(slice[0], slice[1], slice[2], slice[3]);
        }

        public static Rect2 SliceToRows(this Rect rect, float h0, float h1)
        {
            RectSlice slice = new(stackalloc Rect[2]);
            
            slice.Rows(rect, h0, h1);
            
            return new Rect2(slice[0], slice[1]);
        }
        
        public static Rect3 SliceToRows(this Rect rect, float h0, float h1, float h2)
        {
            RectSlice slice = new(stackalloc Rect[3]);
            
            slice.Rows(rect, h0, h1, h2);
            
            return new Rect3(slice[0], slice[1], slice[2]);
        }
        
        public static Rect4 SliceToRows(this Rect rect, float h0, float h1, float h2, float h3)
        {
            RectSlice slice = new(stackalloc Rect[4]);
            
            slice.Rows(rect, h0, h1, h2, h3);
            
            return new Rect4(slice[0], slice[1], slice[2], slice[3]);
        }

        public static Rect AddPadding(this Rect rect, Padding padding)
        {
            Vector2 position = rect.position;
            position.x += padding.left;
            position.y += padding.top;

            Vector2 size = rect.size;
            size.x -= padding.right + padding.left;
            size.y -= padding.down + padding.top;

            return new Rect(position, size);
        }

        public static Rect AddPosition(this Rect rect, float2 position)
        {
            rect.position += (Vector2) position;

            return rect;
        }

        public static Rect AlignCenter(this Rect rect, float2 elementSize)
        {
            float2 position = (float2)rect.position + (float2)rect.size * 0.5f - elementSize * 0.5f;
            return new Rect(position, elementSize);
        }

        public static Rect Expand(this Rect rect, float2 size)
        {
            float2 halfSize = size * 0.5f;

            return new Rect((float2)rect.position - halfSize, (float2)rect.size + halfSize);
        }
    }
}