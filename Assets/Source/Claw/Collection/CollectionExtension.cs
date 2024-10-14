using System.Collections.Generic;

namespace Beast.Claw.Collection
{
    public static class CollectionExtension
    {
        public static T PopOrNull<T>(this IList<T> list) where T : class
        {
            if (list.Count == 0)
            {
                return null;
            }

            T item = list[^1];
            
            list.RemoveAt(list.Count - 1);

            return item;
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

        public static void AddRange<T>(this IList<T> list, IList<T> range)
        {
            foreach (T item in range)
            {
                list.Add(item);
            }
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = new TValue();

            dictionary[key] = value;

            return value;
        }
    }
}