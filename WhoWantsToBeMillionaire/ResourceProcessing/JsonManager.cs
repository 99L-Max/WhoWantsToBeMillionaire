using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    enum TypeResource
    {
        AnimationData,
        Dialogues,
        Dictionaries,
        Questions,
        SettingsValues,
        Sounds,
        Sums,
        Textures
    }

    static class JsonManager
    {
        public static JObject GetObject(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JObject.Parse(jString);
        }

        public static Dictionary<T, string> GetDictionary<T>(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<Dictionary<T, string>>(jString);
        }

        public static T GetObject<T>(byte[] array)
        {
            var jString = Encoding.UTF8.GetString(array);
            return JsonConvert.DeserializeObject<T>(jString);
        }
    }
}
