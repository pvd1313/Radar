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