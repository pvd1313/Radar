using System;
using System.Collections;
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

        public static bool IsIndexInRange(this IList list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static bool NotNullAndHasItems(this IList list)
        {
            return list is { Count: > 0 };
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

        public static int IndexOfReference<TListValue, TReference>(this IList<TListValue> list, TReference reference) 
            where TReference : class
            where TListValue : class
        {
            int itemCount = list.Count;
            if (itemCount == 0)
            {
                return -1;
            }
            
            for (int i = 0; i < itemCount; i++)
            {
                if (ReferenceEquals(reference, list[i]))
                {
                    return i;
                }
            }

            return -1;
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
        
#if UNITY_EDITOR
        /// <summary>
        /// This is very slow operation. As it leads list to copy array elements twice. Use only in Editor.
        /// </summary>
        public static bool TryMoveEditorOnly<TValue>(this IList<TValue> list, int sourceIndex, int targetIndex)
        {
            if (list.Count <= 1)
            {
                return false;
            }

            if (sourceIndex == targetIndex)
            {
                return false;
            }

            int lastIndex = list.Count - 1;
            if (targetIndex < 0 || sourceIndex < 0 || sourceIndex > lastIndex || targetIndex > lastIndex)
            {
                return false;
            }

            list.Insert(targetIndex, list[sourceIndex]);
            
            if (targetIndex < sourceIndex)
            {
                list.RemoveAt(sourceIndex + 1);
            }
            else
            {
                list.RemoveAt(sourceIndex);
            }

            return true;
        }
#endif

        public static void AddRange<TValue>(this IList<TValue> list, IEnumerable<TValue> range)
        {
            foreach (TValue item in range)
            {
                list.Add(item);
            }
        }
    }
}