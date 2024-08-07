﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    static class JsonManager
    {
        public static JObject GetObject(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JObject.Parse(jString);
        }

        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jString);
        }

        public static T GetObject<T>(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<T>(jString);
        }
    }
}
