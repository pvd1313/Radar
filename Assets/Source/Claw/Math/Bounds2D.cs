using Beast.Claw.Serialization;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Math
{
    public struct Bounds2D : ISerializable
    {
        public FloatMinMax x;
        public FloatMinMax y;

        public Bounds2D(FloatMinMax x, FloatMinMax y)
        {
            this.x = x;
            this.y = y;
        }

        public Bounds2D(float xMin, float xMax, float yMin, float yMax)
        {
            x = new FloatMinMax(xMin, xMax);
            y = new FloatMinMax(yMin, yMax);
        }
        
        public Bounds2D(float min, float max)
        {
            x = new FloatMinMax(min, max);
            y = new FloatMinMax(min, max);
        }

        public float2 GetMinPosition()
        {
            return new float2(x.min, y.min);
        }

        public float2 GetMaxPosition()
        {
            return new float2(x.max, y.max);
        }

        public float2 GetCenter()
        {
            return new float2(x.max + x.min, y.max + y.min) * 0.5f;
        }

        public static Bounds2D FromTwoPoints(float2 point0, float2 point1)
        {
            Bounds2D bounds;
            BMath.MinMax(point0.x, point1.x, out bounds.x.min, out bounds.x.max);
            BMath.MinMax(point0.y, point1.y, out bounds.y.min, out bounds.y.max);

            return bounds;
        }

        public static Bounds2D FromPoint(float2 point)
        {
            Bounds2D bounds;
            bounds.x.min = point.x;
            bounds.x.max = point.x;
            bounds.y.min = point.y;
            bounds.y.max = point.y;

            return bounds;
        }

        public static Bounds2D FromPointWithRadius(float2 point, float radius)
        {
            Bounds2D bounds;
            bounds.x.min = point.x - radius;
            bounds.x.max = point.x + radius;
            bounds.y.min = point.y - radius;
            bounds.y.max = point.y + radius;

            return bounds;
        }

        public Rect ToRect(float2 dpi)
        {
            float2 position = new float2(x.min, y.min) * dpi;
            float2 size = new float2(x.GetRange(), y.GetRange()) * dpi;

            return new Rect(position.x, position.y, size.x, size.y);
        }

        public bool Intersects(Bounds2D other)
        {
            return x.Intersects(other.x) && y.Intersects(other.y);
        }

        public bool Contains(float2 point)
        {
            return x.Contains(point.x) && y.Contains(point.y);
        }

        public Bounds2D EnsureInside(Bounds2D border)
        {
            Bounds2D bounds;
            bounds.x.min = BMath.Max(x.min, border.x.min);
            bounds.x.max = BMath.Min(x.max, border.x.max);
            bounds.y.min = BMath.Max(y.min, border.y.min);
            bounds.y.max = BMath.Min(y.max, border.y.max);

            return bounds;
        }

        public Bounds2D EnsureContains(Bounds2D child)
        {
            Bounds2D bounds;
            bounds.x.min = BMath.Min(x.min, child.x.min);
            bounds.x.max = BMath.Max(x.max, child.x.max);
            bounds.y.min = BMath.Min(y.min, child.y.min);
            bounds.y.max = BMath.Max(y.max, child.y.max);

            return bounds;
        }

        public Bounds2D EnsureContains(float2 point)
        {
            Bounds2D bounds;
            bounds.x.min = BMath.Min(x.min, point.x);
            bounds.x.max = BMath.Max(x.max, point.x);
            bounds.y.min = BMath.Min(y.min, point.y);
            bounds.y.max = BMath.Max(y.max, point.y);

            return bounds;
        }

        public readonly bool IsOnBorder(float2 point)
        {
            return x.IsOnBorder(point.x) || y.IsOnBorder(point.y);
        }

        public Bounds2D Shrink(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));

            return new Bounds2D(x.Shrink(size), y.Shrink(size));
        }

        public Bounds2D Expand(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));
            
            return new Bounds2D(x.Expand(size), y.Expand(size));
        }

        public void Serialize(IBinaryWriter stream)
        {
            stream.Write(x);
            stream.Write(y);
        }

        public void Deserialize(IBinaryReader stream)
        {
            stream.Read(ref x);
            stream.Read(ref y);
        }
    }
}