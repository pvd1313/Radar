using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Editor.Serialization
{
    public struct PreferenceBool
    {
        public bool Value
        {
            get
            {
                if (Build.IS_DEVELOPMENT)
                {
                    if (!_isCreated)
                    {
                        throw new NotInitializedException(GetType());
                    }
                }

                return _value;
            }
            set
            {
                if (Build.IS_DEVELOPMENT)
                {
                    if (!_isCreated)
                    {
                        throw new NotInitializedException(GetType());
                    }

                    if (_value != value)
                    {
                        _value = value;

                        Preference.WriteBool(_key, value);
                    }
                }
            }
        }
        
        private readonly bool _isCreated;
        private readonly PreferenceKey _key;
        
        private bool _value;

        public static implicit operator bool(PreferenceBool preferenceBool)
        {
            return preferenceBool._value;
        }
        
        public PreferenceBool(PreferenceKey key, bool defaultValue)
        {
            key.ValidateThrowException(nameof(key));
            
            _key = key;
            
            Preference.ReadBool(key, out _value, defaultValue);

            _isCreated = true;
        }
    }
}