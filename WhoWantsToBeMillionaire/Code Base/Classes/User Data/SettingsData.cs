using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class SettingsData : ISaveable
    {
        [JsonProperty("Settings")]
        private readonly Dictionary<GameSettings, float> _settings;

        public SettingsData()
        {
            _settings = GetDefaultValues();
        }

        [JsonConstructor]
        public SettingsData(Dictionary<GameSettings, float> settings)
        {
            CollectionFactory.AddMissingKeys(settings, GetDefaultValues());
            _settings = settings;
        }

        public float GetSettings(GameSettings key)
        {
            return _settings[key];
        }

        public void ApplyGlobal()
        {
            Sound.SetVolume(_settings[GameSettings.Volume]);
            Music.SetVolume(_settings[GameSettings.Volume]);
        }

        public void Save()
        {
            FileWriter.Save(this, GameDirectory.SettingsFilePath);
        }

        private Dictionary<GameSettings, float> GetDefaultValues()
        {
            return JsonReader.GetDictionary<GameSettings, GameSettingsValues>(Resources.Dictionary_Settings).ToDictionary(pair => pair.Key, pair => pair.Value.DefaultValue);
        }
    }
}
