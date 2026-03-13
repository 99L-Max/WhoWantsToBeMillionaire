using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class AchievementsData : ISaveable
    {
        [JsonProperty("Achievements")]
        private readonly Dictionary<Achievements, bool> _achievements;

        public AchievementsData()
        {
            _achievements = CollectionFactory.GetDefaultEnumDictionary<Achievements, bool>();
        }

        [JsonConstructor]
        public AchievementsData(Dictionary<Achievements, bool> achievements)
        {
            CollectionFactory.AddMissingKeys(achievements);
            _achievements = achievements;
        }

        public bool HaveAllGranted => _achievements.Values.All(_ => _);

        public Dictionary<Achievements, bool> CopyAchievements => new Dictionary<Achievements, bool>(_achievements);

        public bool CheckGranted(Achievements key)
        {
            return _achievements[key];
        }

        public void Grant(Achievements key)
        {
            _achievements[key] = true;
        }

        public void Save()
        {
            FileWriter.Save(this, GameDirectory.AchievementsFilePath);
        }
    }
}
