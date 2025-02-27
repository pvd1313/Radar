using System.Collections;
using System.Collections.Generic;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Validation;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class ListExtension
    {
        public static TValue PopOrNull<TValue>(this IList<TValue> list) where TValue : class
        {
            list.ValidateNotNullThrowException(nameof(list));
            
            if (list.Count == 0)
            {
                return null;
            }

            TValue item = list[^1];
            
            list.RemoveAt(list.Count - 1);

            return item;
        }

        public static void Shuffle<T>(this List<T> list, int seed)
        {
            Unity.Mathematics.Random random = new ((uint)seed);

            for (int i = 0; i < list.Count - 1; i++)
            {
                int j = random.NextInt(i, list.Count);

                if (i != j)
                {
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }

        public static bool IsNullOrEmpty<TValue>(this IList<TValue> list)
        {
            list.ValidateNotNullThrowException(nameof(list));
            
            return list == null || list.Count == 0;
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
                throw new System.InvalidOperationException("No items to pop in list");
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

        public static void AddRange<TValue>(this IList<TValue> list, IEnumerable<TValue> range)
        {
            foreach (TValue item in range)
            {
                list.Add(item);
            }
        }
    }
}