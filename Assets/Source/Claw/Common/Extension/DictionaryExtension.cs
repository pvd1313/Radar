using System.Collections.Generic;
using Beast.Claw.Attributes;

namespace Beast.Claw.Common
{
    public static class DictionaryExtension
    {
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

        public static bool SafeTryGetValueOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary, 
            [CanBeNull] TKey key,
            out TValue value) where TKey: class
        {
            if (key == null)
            {
                value = default;
                return false;
            }

            return dictionary.TryGetValue(key, out value);
        }

        public static TValue InsertAndReturn<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary[key] = value;

            return value;
        }
    }
}