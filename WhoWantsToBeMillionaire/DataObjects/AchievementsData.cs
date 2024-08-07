﻿using Newtonsoft.Json;
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
        private readonly Dictionary<Achievement, bool> _achievements;

        public AchievementsData(string path)
        {
            var keys = Enum.GetValues(typeof(Achievement)).Cast<Achievement>();

            try
            {
                using (var reader = new StreamReader(path + @"\Achievements.json"))
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

        public void Save(string pathSave)
        {
            var data = JsonConvert.SerializeObject(_achievements);
            File.WriteAllText(pathSave + @"\Achievements.json", data);
        }
    }
}
