using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private readonly Dictionary<StatsAttribute, int> attributes;

        public StatisticsData(string path)
        {
            var keys = Enum.GetValues(typeof(StatsAttribute)).Cast<StatsAttribute>();

            try
            {
                using (StreamReader reader = new StreamReader(path + @"\Statistics.json"))
                {
                    string jsonStr = reader.ReadToEnd();
                    attributes = JsonConvert.DeserializeObject<Dictionary<StatsAttribute, int>>(jsonStr);

                    foreach (var key in keys)
                        if (!attributes.ContainsKey(key))
                            attributes.Add(key, 0);
                }
            }
            catch (Exception)
            {
                attributes = keys.ToDictionary(k => k, v => 0);
            }
        }

        public override string ToString()
        {
            var dict = ResourceManager.GetDictionary("Statistics.json");

            return string.Join("\n\n", attributes.Select(at => $"{dict[at.Key.ToString()]}: {String.Format("{0:#,0}", at.Value)}"));
        }

        public void Update(StatsAttribute key, int value = 1) =>
            attributes[key] += value;

        public void Save(string pathSave)
        {
            string data = JsonConvert.SerializeObject(attributes);
            File.WriteAllText(pathSave + @"\Statistics.json", data);
        }
    }
}
