using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Math
{
    [Serializable]
    public struct FloatMinMax : IValidatable
    {
        public float min;
        public float max;

        public FloatMinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Intersects(FloatMinMax other)
        {
            return other.max >= min && max >= other.min;
        }

        public bool Contains(float x)
        {
            return x >= min && x <= max;
        }

        public bool IsNormalized()
        {
            return min >= 0 && max >= 0 && min <= 1 && max <= 1;
        }

        public bool IsValidRange()
        {
            return min <= max;
        }

        public float GetRange()
        {
            return max - min;
        }

        public FloatMinMax Expand(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));

            float halfSize = size * 0.5f;

            return new FloatMinMax(min - halfSize, max + halfSize);
        }

        public FloatMinMax Shrink(float size)
        {
            size.ValidateIsZeroOrGreaterThrowException(nameof(size));
            
            float halfSize = size * 0.5f;

            if (min + halfSize > max)
            {
                float center = (max + min) * 0.5f;

                return new FloatMinMax(center, center);
            }

            return new FloatMinMax(min + halfSize, max - halfSize);
        }

        public float GetCenter()
        {
            return BMath.Lerp(this, 0.5f);
        }

        public string Validate()
        {
            if (min > max)
            {
                return $"Invalid range: {this}";
            }

            return null;
        }

        public override string ToString()
        {
            return $"[{min}, {max}]";
        }
    }
}