using Unity.Collections;

namespace Beast.Claw.Math.CCL
{
    /// <summary>
    /// Allows to union elements by equality. Roots are lowest possible values. Does not use ranking. 
    /// </summary>
    public struct NativeDisjointSet
    {
        private NativeHashMap<int, int> _parent;
        
        public NativeDisjointSet(int capacity, AllocatorManager.AllocatorHandle allocator)
        {
            _parent = new NativeHashMap<int, int>(capacity, allocator);
        }

        public int Find(int value)
        {
            if (!_parent.TryGetValue(value, out int parentValue))
            {
                _parent[value] = value;

                return value;
            }

            if (parentValue != value)
            {
                parentValue = Find(parentValue);
                _parent[value] = parentValue;
            }

            return parentValue;
        }

        public void Union(int x, int y)
        {
            int xRoot = Find(x);
            int yRoot = Find(y);

            if (xRoot == yRoot)
            {
                return;
            }

            if (xRoot < yRoot)
            {
                _parent[yRoot] = xRoot;
            }
            else
            {
                _parent[xRoot] = yRoot;
            }
        }

        public void Dispose()
        {
            _parent.Dispose();
        }
    }
}