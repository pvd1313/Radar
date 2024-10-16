using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public class GUILayoutOptionArrayPool
    {
        private const int MaxArraySize = 8;
        private const int MaxAllocCountPerSize = 8;

        private readonly int[] _allocCount;
        private readonly List<GUILayoutOption[]>[] _pool;

        public GUILayoutOptionArrayPool()
        {
            _allocCount = new int[MaxArraySize];
            _pool = new List<GUILayoutOption[]>[MaxArraySize];

            for (int i = 0; i < MaxArraySize; i++)
            {
                _pool[i] = new List<GUILayoutOption[]>(MaxAllocCountPerSize);
            }
        }
        
        public GUILayoutOption[] GetOrCreate(int arraySize)
        {
            if (arraySize <= 0 || arraySize > MaxArraySize)
            {
                throw new InvalidOperationException("Invalid array size");
            }

            int allocCount = _allocCount[arraySize];
            if (allocCount >= MaxAllocCountPerSize)
            {
                throw new InvalidOperationException("Too many arrays allocated. Are some of them not returned?");
            }

            _allocCount[arraySize] = allocCount + 1;

            List<GUILayoutOption[]> list = _pool[arraySize];
            if (list.TryPop(out GUILayoutOption[] array))
            {
                return array;
            }

            array = new GUILayoutOption[arraySize];

            return array;
        }

        public void Dispose(GUILayoutOption[] array)
        {
            array.ValidateNotNullThrowException(nameof(array));

            int arraySize = array.Length;
            if (arraySize == 0 || arraySize > MaxArraySize)
            {
                throw new InvalidOperationException("Invalid array size. Probably not from this pool :)");
            }
            
            int allocCount = _allocCount[arraySize];
            if (allocCount == 0)
            {
                throw new InvalidOperationException(
                    "All arrays of this size are already in pool. Are you creating new arrays?");
            }

            _allocCount[arraySize] = allocCount - 1;

            _pool[arraySize].Add(array);
        }
    }
}