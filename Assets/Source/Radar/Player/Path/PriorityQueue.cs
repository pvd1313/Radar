using Unity.Collections;

namespace Beast.Radar.Player.Path
{
    public struct PriorityQueue : System.IDisposable
    {
        public int Count => _items.Length;
            
        private NativeList<int> _items;
        private NativeList<int> _priorities;
            
        public PriorityQueue(Allocator allocator)
        {
            _items = new NativeList<int>(allocator);
            _priorities = new NativeList<int>(allocator);
        }

        public void Enqueue(int item, int priority)
        {
            _items.Add(item);
            _priorities.Add(priority);
            _heapifyUp(_items.Length - 1);
        }

        public int Dequeue()
        {
            int item = _items[0];
            _items[0] = _items[^1];
            _priorities[0] = _priorities[^1];
            _items.RemoveAt(_items.Length - 1);
            _priorities.RemoveAt(_priorities.Length - 1);
            _heapifyDown(0);
            return item;
        }

        public void Clear()
        {
            _items.Clear();
            _priorities.Clear();
        }
        
        private void _heapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_priorities[parentIndex] <= _priorities[index]) break;

                _swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void _heapifyDown(int index)
        {
            while (index < _items.Length)
            {
                int smallest = index;
                int left = 2 * index + 1;
                int right = 2 * index + 2;

                if (left < _items.Length && _priorities[left] < _priorities[smallest]) smallest = left;
                if (right < _items.Length && _priorities[right] < _priorities[smallest]) smallest = right;

                if (smallest == index) break;

                _swap(index, smallest);
                index = smallest;
            }
        }

        private void _swap(int indexA, int indexB)
        {
            (_items[indexA], _items[indexB]) = (_items[indexB], _items[indexA]);
            (_priorities[indexA], _priorities[indexB]) = (_priorities[indexB], _priorities[indexA]);
        }

        public void Dispose()
        {
            _items.Dispose();
            _priorities.Dispose();
        }
    }
}