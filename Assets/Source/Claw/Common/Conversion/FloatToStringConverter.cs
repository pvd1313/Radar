using Beast.Claw.Math;

namespace Beast.Claw.Common
{
    public class FloatToStringConverter
    {
        private readonly string[] _fraction;
        private readonly string[] _exponent;

        public FloatToStringConverter(string format)
        {
            _exponent = new string[100];
            for (int i = 0; i < 100; i++)
            {
                _exponent[i] = i.ToString(format);
            }
            
            _fraction = new string[100];
            for (int i = 0; i < 100; i++)
            {
                _fraction[i] = "." + i.ToString(format);
            }
        }
        
        public void ToString(float value, out string exponent, out string fractionWithDot)
        {
            exponent = _exponent[BMath.Min(99, (int) value)];
            fractionWithDot = _fraction[(int) (value % 1 * 100)];
        }
    }
}