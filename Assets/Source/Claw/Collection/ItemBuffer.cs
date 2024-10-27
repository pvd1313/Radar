using System;

namespace Beast.Claw.Collection
{
    public struct ItemBuffer<TValue>
    {
        public int Count => _itemCount;

        public int Capacity => _capacity;
        
        public bool IsInitialized => _isInitialized;

        private readonly TValue[] _buffer;

        private int _capacity;
        private int _itemCount;
        private bool _isInitialized;
        
        public ItemBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new InvalidOperationException("Invalid capacity: " + capacity);
            }
            
            _buffer = new TValue[capacity];
            
            _itemCount = 0;
            _capacity = capacity;
            _isInitialized = true;
        }
        
        public void Clear()
        {
            Array.Fill(_buffer, default, 0, _itemCount);
            
            _itemCount = 0;
        }

        public void InsertItemsAtZeroInReverseOrder(ItemBuffer<TValue> source)
        {
            int finalCount = _itemCount + source.Count;
            
            if (finalCount > _capacity)
            {
                throw new OutOfMemoryException("Not enough capacity to store all items");
            }

            for (int i = 0; i < _itemCount; i++)
            {
                _buffer[finalCount - i - 1] = _buffer[_itemCount - i - 1];
            }

            for (int i = 0; i < source.Count; i++)
            {
                _buffer[i] = source[source.Count - i - 1];
            }

            _itemCount = finalCount;
        }

        public void Add(TValue value)
        {
            _validateIsInitialized();

            if (_itemCount >= _capacity)
            {
                throw new OutOfMemoryException("Not enough capacity to store all items");
            }

            _buffer[_itemCount] = value;
            _itemCount++;
        }

        public TValue this[int index]
        {
            get
            {
                if (index < 0 || index >= _itemCount)
                {
                    throw new IndexOutOfRangeException();
                }

                return _buffer[index];
            }
        }

        private void _validateIsInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Buffer is not initialized");
            }
        }
    }
}