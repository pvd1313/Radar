using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Collections
{
    public struct CellGrid<T> where T : unmanaged, IEquatable<T>
    {
        public float cellSize;
        public NativeParallelMultiHashMap<int2, T> items;

        public CellGrid(float cellSize, int capacity, Allocator allocator)
        {
            this.cellSize = cellSize;
            items = new NativeParallelMultiHashMap<int2, T>(capacity, allocator);
        }

        public void GetCellBounds(Bounds2D worldBounds, out int2 lowerBound, out int2 upperBound)
        {
            lowerBound = _worldPositionToCellIndex(worldBounds.GetMinPosition());
            upperBound = _worldPositionToCellIndex(worldBounds.GetMaxPosition());
        }

        public void AddItem(T item, Bounds2D itemBounds)
        {
            int2 lowerBound = _worldPositionToCellIndex(itemBounds.GetMinPosition());
            int2 upperBound = _worldPositionToCellIndex(itemBounds.GetMaxPosition());

            if (lowerBound.Equals(upperBound))
            {
                items.Add(lowerBound, item);
            }
            else
            {
                for (int x = lowerBound.x; x <= upperBound.x; x++)
                for (int y = lowerBound.y; y <= upperBound.y; y++)
                {
                    items.Add(new int2(x, y), item);
                }
            }
        }

        public void Clear()
        {
            items.Clear();
        }

        public NativeSlice<T> GetItems(Bounds2D searchBounds, NativeSlice<T> buffer)
        {
            int2 lowerBounds = _worldPositionToCellIndex(searchBounds.GetMinPosition());
            int2 upperBound = _worldPositionToCellIndex(searchBounds.GetMaxPosition());

            int itemCount = 0;

            if (lowerBounds.Equals(upperBound))
            {
                _extractCellItems(lowerBounds, buffer, ref itemCount, false);
            }
            else
            {
                for (int x = lowerBounds.x; x <= upperBound.x; x++)
                for (int y = lowerBounds.y; y <= upperBound.y; y++)
                {
                    _extractCellItems(new int2(x, y), buffer, ref itemCount, true);
                }
            }
            
            return buffer.Slice(0, itemCount);
        }

        private void _extractCellItems(int2 cellIndex, NativeSlice<T> buffer, ref int itemCount, bool distinct)
        {
            if (itemCount >= buffer.Length)
            {
                return;
            }
            
            if (!items.TryGetFirstValue(
                    cellIndex,
                    out T item,
                    out NativeParallelMultiHashMapIterator<int2> iterator))
            {
                return;
            }

            if (distinct)
            {
                bool hasItem = _checkBufferContainsItem(buffer, itemCount, item);
                if (!hasItem)
                {
                    buffer[itemCount] = item;
                    itemCount++;
                }
            }
            else
            {
                buffer[itemCount] = item;
                itemCount++;
            }

            while (itemCount < buffer.Length && items.TryGetNextValue(out item, ref iterator))
            {
                if (distinct)
                {
                    bool hasItem = _checkBufferContainsItem(buffer, itemCount, item);
                    if (!hasItem)
                    {
                        buffer[itemCount] = item;
                        itemCount++;
                    }
                }
                else
                {
                    buffer[itemCount] = item;
                    itemCount++;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _checkBufferContainsItem(NativeSlice<T> buffer, int itemCount, T item)
        {
            if (itemCount == 0)
            {
                return false;
            }
            
            for (int i = 0; i < itemCount; i++)
            {
                if (buffer[i].Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        private int2 _worldPositionToCellIndex(float2 position)
        {
            return BMath.FloorToInt(position / cellSize);
        }
        
        public void Dispose()
        {
            if (items.IsCreated)
            {
                items.Dispose();
            }
        }
    }
}