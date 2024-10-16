using System;

namespace Beast.Claw.Math
{
    [Serializable]
    public struct IntMinMax
    {
        public int min;
        public int max;

        public IntMinMax(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public static implicit operator FloatMinMax(IntMinMax minMax)
        {
            return new FloatMinMax(minMax.min, minMax.max);
        }
    }
}