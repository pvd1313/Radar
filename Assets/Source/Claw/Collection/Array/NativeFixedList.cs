using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Unity.Collections;

namespace Beast.Claw.Collection
{
    /// <summary>
    /// Native list with fixed buffer size.
    /// </summary>
    public struct NativeFixedList<T> where T : unmanaged
    {
        public bool IsCreated => _array.IsCreated;
        public int Count => _count;
        
        private NativeArray<T> _array;
        private int _count;
        
        public NativeFixedList(int capacity, Allocator allocator)
        {
            if (capacity < 0)
            {
                throw new ArgumentException(nameof(capacity));
            }

            _array = new NativeArray<T>(capacity, allocator);
            _count = 0;
        }

        public static implicit operator NativeSlice<T>(NativeFixedList<T> list)
        {
            return list.Slice();
        }

        public T this[int index]
        {
            get
            {
                index.ValidateArrayIndex(_count);
                _array.ValidateIsCreated();
                
                return _array[index];
            }
            set
            {
                index.ValidateArrayIndex(_count);
                _array.ValidateIsCreated();
                
                _array[index] = value;
            }
        }

        public void Add(T value)
        {
            _array.ValidateIsCreated();
            
            if (Build.IS_DEVELOPMENT)
            {
                if (_count >= _array.Length)
                {
                    throw new OutOfMemoryException(GetType().FullName);
                }
            }

            _array[_count] = value;

            _count++;
        }

        public void ResetCount()
        {
            _count = 0;
        }

        public NativeSlice<T> Slice()
        {
            return _array.Slice(0, _count);
        }

        public void SafeDispose()
        {
            if (_array.IsCreated)
            {
                _array.Dispose();
            }
        }
    }
}