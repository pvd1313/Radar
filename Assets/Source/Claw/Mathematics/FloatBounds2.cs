using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Mathematics
{
    public struct FloatBounds2
    {
        public FloatMinMax x;
        public FloatMinMax y;

        public FloatBounds2(FloatMinMax x, FloatMinMax y)
        {
            this.x = x;
            this.y = y;
        }

        public FloatBounds2(float xMin, float xMax, float yMin, float yMax)
        {
            x = new FloatMinMax(xMin, xMax);
            y = new FloatMinMax(yMin, yMax);
        }
        
        public FloatBounds2(float min, float max)
        {
            x = new FloatMinMax(min, max);
            y = new FloatMinMax(min, max);
        }

        public Rect ToRect(float2 dpi)
        {
            float2 position = new float2(x.min, y.min) * dpi;
            float2 size = new float2(x.GetRange(), y.GetRange()) * dpi;

            return new Rect(position.x, position.y, size.x, size.y);
        }

        public bool Intersects(FloatBounds2 other)
        {
            return x.Intersects(other.x) && y.Intersects(other.y);
        }

        public bool Contains(float2 point)
        {
            return x.Contains(point.x) && y.Contains(point.y);
        }

        public FloatBounds2 Shrink(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));

            return new FloatBounds2(x.Shrink(size), y.Shrink(size));
        }

        public FloatBounds2 Expand(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));
            
            return new FloatBounds2(x.Expand(size), y.Expand(size));
        }
    }
}