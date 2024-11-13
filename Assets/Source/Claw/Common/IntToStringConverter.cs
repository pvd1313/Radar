namespace Beast.Claw.Common
{
    public class IntToStringConverter
    {
        private readonly string[] _intString;

        public IntToStringConverter(int maxRange, string format = "D")
        {
            _intString = new string[maxRange];
            
            for (int i = 0; i < maxRange; i++)
            {
                _intString[i] = i.ToString(format);
            }
        }

        public string Convert(int value)
        {
            if (value >= 0 && value < _intString.Length)
            {
                return _intString[value];
            }

            return "N/A";
        }
    }
}