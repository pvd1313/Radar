using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Mathematics
{
    public struct FloatBounds2D
    {
        public FloatMinMax x;
        public FloatMinMax y;

        public FloatBounds2D(FloatMinMax x, FloatMinMax y)
        {
            this.x = x;
            this.y = y;
        }

        public FloatBounds2D(float xMin, float xMax, float yMin, float yMax)
        {
            x = new FloatMinMax(xMin, xMax);
            y = new FloatMinMax(yMin, yMax);
        }
        
        public FloatBounds2D(float min, float max)
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

        public bool Intersects(FloatBounds2D other)
        {
            return x.Intersects(other.x) && y.Intersects(other.y);
        }

        public bool Contains(float2 point)
        {
            return x.Contains(point.x) && y.Contains(point.y);
        }

        public FloatBounds2D Shrink(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));

            return new FloatBounds2D(x.Shrink(size), y.Shrink(size));
        }

        public FloatBounds2D Expand(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));
            
            return new FloatBounds2D(x.Expand(size), y.Expand(size));
        }
    }
}