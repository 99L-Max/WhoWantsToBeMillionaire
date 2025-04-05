using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class StatisticsData : GameData
    {
        private readonly Dictionary<StatisticsAttribute, int> _attributes;

        public StatisticsData(string path) : base(path, "Statistics.json")
        {
            var keys = Enum.GetValues(typeof(StatisticsAttribute)).Cast<StatisticsAttribute>();

            try
            {
                using (StreamReader reader = new StreamReader(FullPathFile))
                {
                    var jString = reader.ReadToEnd();
                    _attributes = JsonConvert.DeserializeObject<Dictionary<StatisticsAttribute, int>>(jString);

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
            var dict = JsonManager.GetDictionary<StatisticsAttribute, string>(Resources.Dictionary_Statistics);
            return string.Join("\n\n", _attributes.Select(a => $"{dict[a.Key]}: {GetIntFormat(a.Value, 999999999, "{0:#,0}")}"));
        }

        private string GetIntFormat(int value, int maxValue, string format)
        {
            if (value < maxValue) return string.Format(format, value);
            return string.Format($"{format}+", maxValue);
        }

        public int GetAttribute(StatisticsAttribute key) =>
            _attributes[key];

        public void Update(StatisticsAttribute key, int value = 1) =>
            _attributes[key] += value;

        public override void Save() =>
            Save(_attributes);
    }
}
