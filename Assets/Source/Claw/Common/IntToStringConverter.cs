namespace Beast.Claw.Common
{
    public class IntToStringConverter
    {
        private readonly string[] _intStrings;

        public IntToStringConverter(int maxRange = 1000, string format = "D")
        {
            _intStrings = new string[maxRange];
            for (int i = 0; i < maxRange; i++)
            {
                _intStrings[i] = i.ToString(format);
            }
        }

        public string ToString(int value)
        {
            if (value >= 0 && value < _intStrings.Length)
            {
                return _intStrings[value];
            }
            return value.ToString();
        }
    }
}