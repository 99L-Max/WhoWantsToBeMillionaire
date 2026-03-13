using System;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    static class CollectionFactory
    {
        public static T[] GetEnum<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public static Dictionary<TKey, TValue> GetDefaultEnumDictionary<TKey, TValue>() where TKey : Enum
        {
            return GetEnum<TKey>().ToDictionary(_ => _, _ => default(TValue));
        }

        public static Dictionary<TKey, TValue> JoinToDictionary<TKey, TValue>(IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            return keys.Zip(values, (key, value) => new { Key = key, Value = value }).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static void AddMissingKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : Enum
        {
            foreach (var key in GetEnum<TKey>())
                if (dictionary.ContainsKey(key) == false)
                    dictionary.Add(key, default);
        }

        public static void AddMissingKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> defaultValues)
        {
            foreach (var pair in defaultValues)
                if (dictionary.ContainsKey(pair.Key) == false)
                    dictionary.Add(pair.Key, pair.Value);
        }
    }
}
