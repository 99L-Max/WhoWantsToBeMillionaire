using Newtonsoft.Json;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    static class FileWriter
    {
        public static void Save(object data, string path)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }
    }
}
