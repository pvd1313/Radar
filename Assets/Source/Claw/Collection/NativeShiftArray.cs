using System;
using Unity.Collections;

namespace Beast.Claw.Collection
{
    /// <summary>
    /// This array remembers only last N elements.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public struct NativeShiftArray<TValue> : IDisposable where TValue : struct
    {
        public int Count => _count;

        private int _count;
        private NativeArray<TValue> _array;
        private int _first;
        
        public NativeShiftArray(int capacity, Allocator allocator)
        {
            _array = new NativeArray<TValue>(capacity, allocator);
            _count = 0;
            _first = 0;
        }

        public void Add(TValue value)
        {
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
                if (index < 0 || index >= _count)
                {
                    throw new IndexOutOfRangeException();
                }
                
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