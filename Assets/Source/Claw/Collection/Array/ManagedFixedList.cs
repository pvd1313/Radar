using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Collection
{
    public struct ManagedFixedList<TValue>
    {
        public int Count => _count;

        public int Capacity => _capacity;
        
        private readonly TValue[] _buffer;

        private readonly int _capacity;
        private readonly bool _isInitialized;
        private int _count;
        
        public ManagedFixedList(int capacity)
        {
            if (capacity <= 0)
            {
                throw new InvalidOperationException("Invalid capacity: " + capacity);
            }
            
            _buffer = new TValue[capacity];
            
            _count = 0;
            _capacity = capacity;
            _isInitialized = true;
        }
        
        public void ResetCount()
        {
            _count = 0;
        }

        public void InsertItemsAtZeroInReverseOrder(ManagedFixedList<TValue> source)
        {
            _validateIsInitialized();
            source._validateIsInitialized();
            
            int finalCount = _count + source.Count;
            
            if (finalCount > _capacity)
            {
                throw new OutOfMemoryException("Not enough capacity to store all items");
            }

            for (int i = 0; i < _count; i++)
            {
                _buffer[finalCount - i - 1] = _buffer[_count - i - 1];
            }

            for (int i = 0; i < source.Count; i++)
            {
                _buffer[i] = source[source.Count - i - 1];
            }

            _count = finalCount;
        }

        public void Add(TValue value)
        {
            _validateIsInitialized();

            if (Build.IS_DEVELOPMENT)
            {
                if (_count >= _capacity)
                {
                    throw new OutOfMemoryException("Not enough capacity to store all items");
                }
            }

            _buffer[_count] = value;
            _count++;
        }

        public TValue this[int index]
        {
            get
            {
                _validateIsInitialized();
                
                index.ValidateArrayIndex(_count);

                return _buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("DEVELOPMENT")]
        private void _validateIsInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Buffer is not initialized");
            }
        }
    }
}