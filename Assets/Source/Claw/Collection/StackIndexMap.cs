using System;
using System.Collections;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Collection
{
    public ref struct StackIndexMap<TTarget> where TTarget : class
    {
        public int Count => _foundCount;
        
        private readonly Span<short> _indexMap;
        
        private IList _sourceList;
        private short _foundCount;
        private bool _isReady;

        public StackIndexMap(Span<short> indexMap)
        {
            _indexMap = indexMap;
            _sourceList = null;
            _foundCount = 0;
            _isReady = false;
        }
        
        public int FindAllWithTypeAndCurrent<TList>(IList<TList> list, TTarget target)
        {
            list.ValidateNotNullThrowException(nameof(list));
            if (list.Count - 1 >= short.MaxValue)
            {
                throw new InvalidOperationException("Too big list");
            }

            _sourceList = (IList) list;

            _foundCount = 0;
            int indexOfTarget = -1;

            for (short i = 0; i < list.Count && _foundCount < _indexMap.Length; i++)
            {
                if (list[i] is not TTarget listItemAsTarget)
                {
                    continue;
                }

                if (_foundCount >= _indexMap.Length)
                {
                    throw new Exception("Not enough space in span to store all indexes");
                }

                if (indexOfTarget == -1 && ReferenceEquals(listItemAsTarget, target))
                {
                    indexOfTarget = _foundCount;
                }

                _indexMap[_foundCount] = i;

                _foundCount++;
            }

            _isReady = true;

            return indexOfTarget;
        }

        public TTarget this[int index]
        {
            get
            {
                if (!_isReady)
                {
                    throw new InvalidOperationException("ListIndexMap was not initialized");
                }
                
                if (index < 0 || index > _foundCount)
                {
                    throw new IndexOutOfRangeException();
                }

                return (TTarget)_sourceList[_indexMap[index]];
            }
        }
    }
}