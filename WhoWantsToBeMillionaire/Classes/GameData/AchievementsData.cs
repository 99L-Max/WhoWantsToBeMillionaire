using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WhoWantsToBeMillionaire
{
    class AchievementsData : GameData
    {
        private readonly Dictionary<Achievement, bool> _achievements;

        public AchievementsData(string path) : base(path, "Achievements.json")
        {
            var keys = Enum.GetValues(typeof(Achievement)).Cast<Achievement>();

            try
            {
                using (var reader = new StreamReader(FullPathFile))
                {
                    var jString = reader.ReadToEnd();
                    _achievements = JsonConvert.DeserializeObject<Dictionary<Achievement, bool>>(jString);

                    foreach (var key in keys)
                        if (!_achievements.ContainsKey(key))
                            _achievements.Add(key, false);
                }
            }
            catch (Exception)
            {
                _achievements = keys.ToDictionary(k => k, v => false);
            }
        }

        public bool AllGranted =>
            _achievements.Values.All(x => x);

        public Dictionary<Achievement, bool> Achievements =>
            _achievements.ToDictionary(k => k.Key, v => v.Value);

        public bool CheckGranted(Achievement key) =>
            _achievements[key];

        public void Grant(Achievement key) =>
            _achievements[key] = true;

        public override void Save() =>
            Save(_achievements);
    }
}
