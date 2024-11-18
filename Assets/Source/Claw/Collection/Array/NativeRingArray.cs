using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Unity.Collections;

namespace Beast.Claw.Collection
{
    /// <summary>
    /// This array remembers only last N elements.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public struct NativeRingArray<TValue> : IDisposable where TValue : unmanaged
    {
        public int Count => _count;

        private int _count;
        private NativeArray<TValue> _array;
        private int _first;
        
        public NativeRingArray(int capacity, Allocator allocator)
        {
            _array = new NativeArray<TValue>(capacity, allocator);
            _count = 0;
            _first = 0;
        }

        public void Add(TValue value)
        {
            _array.ValidateIsCreated();
            
            if (_count >= _array.Length)
            {
                _array[_first] = value;
                _first = (_first + 1) % _array.Length;
                return;
            }

            _array[_count] = value;
            _count++;
        }

        public TValue this[int index]
        {
            get
            {
                index.ValidateArrayIndex(_count);
                _array.ValidateIsCreated();
                
                index = (_first + index) % _array.Length;

                return _array[index];
            }
        }

        public void Dispose()
        {
            if (_array.IsCreated)
            {
                _array.Dispose();
            }
        }
    }
}