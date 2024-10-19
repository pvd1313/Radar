using System;
using System.Collections.Generic;

namespace Beast.Claw.Collection
{
    public static class ListExtension
    {
        public static TValue PopOrNull<TValue>(this IList<TValue> list) where TValue : class
        {
            if (list.Count == 0)
            {
                return null;
            }

            TValue item = list[^1];
            
            list.RemoveAt(list.Count - 1);

            return item;
        }

        public static TValue Pop<TValue>(this IList<TValue> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("No items to pop in list");
            }
            
            TValue item = list[^1];
            
            list.RemoveAt(list.Count - 1);

            return item;
        }

        public static bool TryPop<TValue>(this IList<TValue> list, out TValue item)
        {
            if (list.Count == 0)
            {
                item = default;
                
                return false;
            }

            item = list[^1];

            list.RemoveAt(list.Count - 1);

            return true;
        }

        public static bool TryRemoveAt<TValue>(this IList<TValue> list, int index)
        {
            if (list.Count == 0)
            {
                return false;
            }

            if (index < 0 || index >= list.Count)
            {
                return false;
            }
            
            list.RemoveAt(index);

            return true;
        }

        public static bool TrySwap<TValue>(this IList<TValue> list, int indexA, int indexB)
        {
            if (list.Count == 0)
            {
                return false;
            }

            if (indexA == indexB)
            {
                return false;
            }

            int lastIndex = list.Count - 1;
            if (indexA < 0 || indexB < 0 || indexA > lastIndex || indexB > lastIndex)
            {
                return false;
            }
            
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);

            return true;
        }

        public static void AddRange<TValue>(this IList<TValue> list, IEnumerable<TValue> range)
        {
            foreach (TValue item in range)
            {
                list.Add(item);
            }
        }
    }
}