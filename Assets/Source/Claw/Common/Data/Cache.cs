using System;

namespace Beast.Claw.Common
{
    public struct Cache<T> where T : IEquatable<T>
    {
        public T Value => _value;
        
        private byte _isInitialized;
        private T _value;

        public bool Update(T newValue)
        {
            if (_isInitialized == 1)
            {
                if (_value.Equals(newValue))
                {
                    return false;
                }

                _value = newValue;
                return true;
            }

            _isInitialized = 1;
            _value = newValue;
            
            return true;
        }

        public static implicit operator T(Cache<T> cache)
        {
            return cache._value;
        }
    }
}