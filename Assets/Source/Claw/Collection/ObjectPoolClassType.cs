using System;
using Beast.Claw.Reflection;

namespace Beast.Claw.Collection
{
    public class ObjectPoolClassType<TValue> where TValue : class, new()
    {
        private readonly TValue[] _pool;
        private readonly int _capacity;
        private readonly bool _isInitialized;
        
        private int _index;
        
        public ObjectPoolClassType(int capacity)
        {
            if (capacity < 0 || capacity > TempConfig.SAFE_CAPACITY)
            {
                throw new ArgumentException("Invalid capacity " + capacity);
            }

            _capacity = capacity;
            
            _pool = new TValue[_capacity];
            
            for (int i = 0; i < _capacity; i++)
            {
                _pool[i] = new TValue();
            }

            _index = 0;
            _isInitialized = true;
        }

        public TValue Next()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{this.TypeName()} was not initialized.");
            }
            
            TValue value = _pool[_index];

            _index = (_index + 1) % _capacity;

            return value;
        }
    }
}