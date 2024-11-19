using System;
using Beast.Claw.Validation;

namespace Beast.Claw.Text
{
    /// <summary>
    /// Cashed string with predefined format. Update() must be called to update inner string.
    /// </summary>
    public class StringWithFormat
    {
        public string Value => _value;
        public EStringFormat Format => _format;

        private readonly EStringFormat _format;

        private string _value;
        private string _fromValue;
        
        public static implicit operator string(StringWithFormat withFormat)
        {
            return withFormat._value;
        }

        public StringWithFormat(string source, EStringFormat format)
        {
            source.ValidateNotNullThrowException(nameof(source));

            _format = format;

            Update(source);
        }

        public void Update(string source)
        {
            source.ValidateNotNullThrowException(nameof(source));

            if (_fromValue == source)
            {
                return;
            }

            switch (_format)
            {
                case EStringFormat.None:
                    _value = source;
                    break;
                
                case EStringFormat.LowerCase:
                    _value = source.ToLower();
                    break;
                
                case EStringFormat.UpperCase:
                    _value = source.ToUpper();
                    break;
                
                default:
                    throw new Exception("Unknown format " + _format.Q());
            }

            _fromValue = source;
        }
    }
}