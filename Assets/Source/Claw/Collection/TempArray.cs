using System;

namespace Beast.Claw.Collection
{
    public readonly struct TempArray<TValue>
    {
        private const int MAX_SAFE_CAPACITY = 64;

        public int Capacity => _capacity;

        private readonly TValue[][] _array;
        private readonly int _capacity;
        private readonly bool _isInitialized;

        public TempArray(int capacity)
        {
            if (capacity < 0 || capacity > MAX_SAFE_CAPACITY)
            {
                throw new ArgumentException("Invalid capacity: " + capacity);
            }

            _capacity = capacity;

            _array = new TValue[_capacity + 1][];

            for (int i = 0; i < _capacity + 1; i++)
            {
                _array[i] = new TValue[i];
            }

            _isInitialized = true;
        }
        
        public TValue[] this[int arrayLength]
        {
            get
            {
                if (!_isInitialized)
                {
                    throw new InvalidCastException("TempArray was not initialized");
                }
            
                if (arrayLength < 0 || arrayLength > _capacity)
                {
                    throw new InvalidOperationException("Invalid arrayLength: " + arrayLength);
                }

                return _array[arrayLength];
            }
        }
    }
}