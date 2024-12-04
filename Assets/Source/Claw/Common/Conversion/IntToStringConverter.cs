using System;
using Beast.Claw.Debug;

namespace Beast.Claw.Common
{
    public class IntToStringConverter
    {
        private readonly string[] _intString;
        private readonly int _offset;

        public IntToStringConverter(int min,int max, string format = "D")
        {
            if (min >= max)
            {
                BDebug.LogException(new ArgumentException("Min must be less than max"));
            }

            _offset = -min; 
            _intString = new string[max - min];

            for (int i = min; i < max; i++)
            {
                _intString[i + _offset] = i.ToString(format);
            }
        }

        public string Convert(int value)
        {
            value += _offset;
            
            if (value >= 0 && value < _intString.Length)
            {
                return _intString[value];
            }

            return "N/A";
        }
    }
}