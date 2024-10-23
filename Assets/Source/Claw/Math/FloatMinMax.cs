using System;

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

        public FloatMinMax Shrink(float size)
        {
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