using System;

namespace Beast.Claw.Math
{
    [Serializable]
    public struct FloatMinMax
    {
        public float min;
        public float max;

        public FloatMinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}