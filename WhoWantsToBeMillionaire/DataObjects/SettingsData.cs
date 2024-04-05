using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

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
            var defaultSettings = JsonManager.GetDictionary<GameSettings, float>(Resources.Settings_Default);

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Settings.json"))
                {
                    var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();
                    var jsonStr = reader.ReadToEnd();

                    _settings = JsonConvert.DeserializeObject<Dictionary<GameSettings, float>>(jsonStr);

                    foreach (var key in keys)
                        if (!_settings.ContainsKey(key))
                            _settings.Add(key, defaultSettings[key]);
                }
            }
            catch (Exception)
            {
                _settings = defaultSettings;
            }
        }

        public GameSettingsData(Dictionary<GameSettings, float> settings) =>
            _settings = settings;

        public float GetSettings(GameSettings key) =>
            _settings[key];

        public void ApplyGlobal() =>
            Sound.SetVolume(_settings[GameSettings.Volume]);

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(pathSave + @"\Settings.json", data);
        }
    }
}
