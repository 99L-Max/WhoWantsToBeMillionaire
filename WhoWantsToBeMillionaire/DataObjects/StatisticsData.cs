using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum StatsAttribute
    {
        TotalPrize,
        NumberCorrectAnswers,
        NumberIncorrectAnswers,
        NumberHintsUsed
    }

    class StatisticsData
    {
        private readonly Dictionary<StatsAttribute, int> _attributes;

        public StatisticsData(string path)
        {
            var keys = Enum.GetValues(typeof(StatsAttribute)).Cast<StatsAttribute>();

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Statistics.json"))
                {
                    string jsonStr = reader.ReadToEnd();
                    _attributes = JsonConvert.DeserializeObject<Dictionary<StatsAttribute, int>>(jsonStr);

                    foreach (var key in keys)
                        if (!_attributes.ContainsKey(key))
                            _attributes.Add(key, 0);
                }
            }
            catch (Exception)
            {
                _attributes = keys.ToDictionary(k => k, v => 0);
            }
        }

        public override string ToString()
        {
            var dict = JsonManager.GetDictionary<StatsAttribute>(Resources.Dictionary_Statistics);
            return string.Join("\n\n", _attributes.Select(a => $"{dict[a.Key]}: {string.Format("{0:#,0}", a.Value)}"));
        }

        public int GetAttribute(StatsAttribute key) => 
            _attributes[key];

        public void Update(StatsAttribute key, int value = 1) =>
            _attributes[key] += value;

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(_attributes);
            File.WriteAllText(pathSave + @"\Statistics.json", data);
        }
    }
}
