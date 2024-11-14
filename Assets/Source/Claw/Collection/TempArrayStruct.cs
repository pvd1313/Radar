using System;
using Beast.Claw.Common;
using Beast.Claw.Reflection;

namespace Beast.Claw.Collection
{
    public readonly struct TempArrayStruct<TValue>
    {
        public int Capacity => _capacity;

        private readonly TValue[][] _array;
        private readonly int _capacity;
        private readonly bool _isInitialized;

        public TempArrayStruct(int capacity)
        {
            if (capacity < 0 || capacity > Build.SMALL_ARRAY_SAFE_CAPACITY)
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
                    throw new InvalidCastException($"{this.TypeName()} was not initialized");
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