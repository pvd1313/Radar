using System;
using Beast.Claw.Common;
using Unity.Collections;

namespace Beast.Claw.Collection
{
    /// <summary>
    /// Native list with fixed buffer size.
    /// </summary>
    public struct NativeFixedList<T> : IDisposable where T : unmanaged
    {
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
                if (Build.IS_DEVELOPMENT)
                {
                    if (index < 0 || index > _count)
                    {
                        throw new IndexOutOfRangeException(GetType().FullName);
                    }
                }
                
                return _array[index];
            }
            set
            {
                if (Build.IS_DEVELOPMENT)
                {
                    if (index < 0 || index > _count)
                    {
                        throw new IndexOutOfRangeException(GetType().FullName);
                    }
                }

                _array[index] = value;
            }
        }

        public void Add(T value)
        {
            if (Build.IS_DEVELOPMENT)
            {
                if (!_array.IsCreated)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                
                if (_count >= _array.Length)
                {
                    throw new OutOfMemoryException(GetType().FullName);
                }
            }

            _array[_count] = value;

            _count++;
        }

        public void Clear()
        {
            _count = 0;
        }

        public NativeSlice<T> Slice()
        {
            return _array.Slice(0, _count);
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