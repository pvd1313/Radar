using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Validation;

namespace Beast.Claw.Common
{
    public static class DictionaryExtension
    {
        public static TValue GetOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            dictionary.ValidateNotNullThrowException(nameof(dictionary));
            
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = new TValue();

            dictionary[key] = value;

            return value;
        }

        public static TValue GetOrCreateWithFactory<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> factory)
        {
            factory.ValidateNotNullThrowException(nameof(factory));
            dictionary.ValidateNotNullThrowException(nameof(dictionary));
            
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = factory.Invoke(key);

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