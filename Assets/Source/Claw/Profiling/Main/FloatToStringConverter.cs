using Beast.Claw.Mathematics;

namespace Beast.Claw.Profiling
{
    public class FloatToStringConverter
    {
        private readonly string[] _fraction;
        private readonly string[] _exponent;

        public FloatToStringConverter()
        {
            _fraction = new string[100];
            for (int i = 0; i < 100; i++)
            {
                _fraction[i] = "." + i.ToString("D2");
            }
            
            _exponent = new string[100];
            for (int i = 0; i < 100; i++)
            {
                _exponent[i] = i.ToString("D2");
            }
        }
        
        public void ToString(float value, out string exponent, out string fraction)
        {
            exponent = _exponent[BMath.Min(99, (int) value)];
            fraction = _fraction[(int) (value % 1 * 100)];
        }
    }
}