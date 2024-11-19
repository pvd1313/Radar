using System;
using Beast.Claw.Common;
using Beast.Claw.Text;
using Beast.Claw.Validation;

namespace Beast.Claw.Collections
{
    /// <summary>
    /// Uses System.Array as buffer.
    /// </summary>
    public class RingPool<TValue>
    {
        public int Capacity => _capacity;
        
        private readonly TValue[] _pool;
        private readonly int _capacity;
        private readonly bool _isInitialized;

        private readonly object _lock;
        
        private int _index;

        public RingPool(int capacity, Func<TValue> factory)
        {
            if (Build.IS_DEVELOPMENT)
            {
                if (capacity < 0 || capacity > Build.SMALL_ARRAY_SAFE_CAPACITY)
                {
                    throw new ArgumentException("Invalid capacity " + capacity);
                }
            }
            
            factory.ValidateNotNullThrowException(nameof(factory));

            _lock = new object();
            _capacity = capacity;
            
            _pool = new TValue[_capacity];
            
            for (int i = 0; i < _capacity; i++)
            {
                _pool[i] = factory.Invoke();
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