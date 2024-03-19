using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

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

    static class ResourceManager
    {
        public static Stream GetStream(string resourceName, TypeResource type)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"WhoWantsToBeMillionaire.Resources.{type}.{resourceName}");
        }

        public static Image GetImage(string resourceName)
        {
            using (Stream stream = GetStream(resourceName, TypeResource.Textures))
                return Image.FromStream(stream);
        }

        public static string GetDialog(string fileName)
        {
            using (Stream stream = GetStream(fileName, TypeResource.Dialogues))
            using (StreamReader reader = new StreamReader(stream))
                 return reader.ReadToEnd();
        }

        public static Dictionary<string, string> GetDictionary(string fileName)
        {
            using (Stream stream = GetStream(fileName, TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonStr = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            }
        }
    }
}
