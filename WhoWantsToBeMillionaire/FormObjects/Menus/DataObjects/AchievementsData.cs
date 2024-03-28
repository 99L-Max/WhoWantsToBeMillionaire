using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum Achievement
    {
        IsPossible,
        StopGame,
        MoneyNotBurn,
        ExcessiveСaution,
        DearComputer,
        AndToTalk,
        AudienceAward,
        SuccessfulOutcome,
        DefectiveQuestion,
        NoOneWillKnow,
        WasTwoBecameFour,
        NoOptions,
        Millionaire,
        Jubilee,
        TriumphReason
    }

    class AchievementsData
    {
        private readonly Dictionary<Achievement, bool> achievements;

        public Dictionary<Achievement, bool> Achievements => achievements.ToDictionary(k => k.Key, v => v.Value);

        public AchievementsData(string path)
        {
            var keys = Enum.GetValues(typeof(Achievement)).Cast<Achievement>();

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Achievements.json"))
                {
                    string jsonStr = reader.ReadToEnd();
                    achievements = JsonConvert.DeserializeObject<Dictionary<Achievement, bool>>(jsonStr);

                    foreach (var key in keys)
                        if (!achievements.ContainsKey(key))
                            achievements.Add(key, false);
                }
            }
            catch (Exception)
            {
                achievements = keys.ToDictionary(k => k, v => false);
            }
        }

        public bool CheckGranted(Achievement key) => achievements[key];

        public void Grant(Achievement key) => achievements[key] = true;

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(achievements);
            File.WriteAllText(pathSave + @"\Achievements.json", data);
        }
    }
}
