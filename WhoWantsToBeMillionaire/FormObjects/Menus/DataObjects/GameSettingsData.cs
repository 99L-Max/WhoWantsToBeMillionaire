using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum GameSettings
    {
        ShowScreensaver,
        ShowOptionsSequentially,
        Volume
    }

    class GameSettingsData
    {
        private readonly Dictionary<GameSettings, float> settings;

        public GameSettingsData(string path)
        {
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Settings.json"))
                {
                    string jsonStr = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Dictionary<GameSettings, float>>(jsonStr);

                    foreach (var key in keys)
                        if (!settings.ContainsKey(key))
                            settings.Add(key, 0f);
                }
            }
            catch (Exception)
            {
                settings = keys.ToDictionary(k => k, v => 0f);
            }
        }

        public void Update(GameSettings key, float value) => settings[key] = value;

        public object GetSettings(GameSettings key) => settings[key];

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(settings);
            File.WriteAllText(pathSave + @"\Settings.json", data);
        }
    }
}
