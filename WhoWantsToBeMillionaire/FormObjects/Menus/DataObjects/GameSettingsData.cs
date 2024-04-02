using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum GameSettings
    {
        Volume,
        ShowScreensaver,
        ShowOptionsSequentially,
        ShowDescriptionHints
    }

    class GameSettingsData
    {
        private readonly Dictionary<GameSettings, float> _settings;

        public GameSettingsData(string path)
        {
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Settings.json"))
                {
                    string jsonStr = reader.ReadToEnd();
                    _settings = JsonConvert.DeserializeObject<Dictionary<GameSettings, float>>(jsonStr);

                    foreach (var key in keys)
                        if (!_settings.ContainsKey(key))
                            _settings.Add(key, 0f);
                }
            }
            catch (Exception)
            {
                _settings = keys.ToDictionary(k => k, v => 1f);
            }
        }

        public GameSettingsData(Dictionary<GameSettings, float> settings) =>
            _settings = settings;

        public float GetSettings(GameSettings key) =>
            _settings[key];

        public void ApplyGlobal() =>
            Sound.SetVolume(_settings[GameSettings.Volume] / 10f);

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(pathSave + @"\Settings.json", data);
        }
    }
}
