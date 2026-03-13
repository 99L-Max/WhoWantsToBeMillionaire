using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    static class JsonReader
    {
        public static JObject GetObject(byte[] array)
        {
            var json = Encoding.UTF8.GetString(array);
            return JObject.Parse(json);
        }

        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(byte[] array)
        {
            var json = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
        }

        public static T GetObject<T>(byte[] array)
        {
            var json = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
