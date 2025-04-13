using System.IO;
using Newtonsoft.Json;

namespace WhoWantsToBeMillionaire
{
    abstract class GameData
    {
        public readonly string FullPathFile;

        public GameData(string path, string fileName) =>
            FullPathFile = $@"{path}\{fileName}";

        protected void Save(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(FullPathFile, json);
        }

        public abstract void Save();
    }
}
