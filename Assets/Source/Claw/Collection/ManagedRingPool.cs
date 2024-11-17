using System;
using Beast.Claw.Common;
using Beast.Claw.Reflection;

namespace Beast.Claw.Collection
{
    public class ManagedRingPool<TValue> where TValue : class, new()
    {
        private readonly TValue[] _pool;
        private readonly int _capacity;
        private readonly bool _isInitialized;

        private readonly object _lock;
        
        private int _index;
        
        public ManagedRingPool(int capacity)
        {
            if (capacity < 0 || capacity > Build.SMALL_ARRAY_SAFE_CAPACITY)
            {
                throw new ArgumentException("Invalid capacity " + capacity);
            }

            _lock = new object();
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
                throw new InvalidOperationException($"{GetType().FullName.Q()} was not initialized.");
            }

            lock (_lock)
            {
                TValue value = _pool[_index];

                _index = (_index + 1) % _capacity;

                return value;
            }
        }
    }
}