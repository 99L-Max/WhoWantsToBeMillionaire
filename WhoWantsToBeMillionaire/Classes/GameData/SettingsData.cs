using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class SettingsData : GameData
    {
        private readonly Dictionary<GameSettings, float> _settings;

        public SettingsData(string path, Dictionary<GameSettings, float> settings = null) : base(path, "Settings.json")
        {
            var defaultSettings = JsonManager.GetDictionary<GameSettings, float>(Resources.Settings_Default);
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();

            if (settings == null)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(FullPathFile))
                    {
                        var jString = reader.ReadToEnd();
                        _settings = JsonConvert.DeserializeObject<Dictionary<GameSettings, float>>(jString);
                    }
                }
                catch (Exception)
                {
                    _settings = defaultSettings;
                }
            }
            else
            {
                _settings = settings;
            }

            foreach (var key in keys)
                if (!_settings.ContainsKey(key))
                    _settings.Add(key, defaultSettings[key]);
        }

        public float GetSettings(GameSettings key) =>
            _settings[key];

        public void ApplyGlobal()
        {
            GameSound.SetVolume(_settings[GameSettings.Volume]);
            GameMusic.SetVolume(_settings[GameSettings.Volume]);
        }

        public override void Save() =>
            Save(_settings);
    }
}
