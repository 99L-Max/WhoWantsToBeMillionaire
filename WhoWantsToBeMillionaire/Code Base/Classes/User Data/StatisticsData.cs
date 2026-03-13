using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class StatisticsData : ISaveable
    {
        [JsonProperty("Attributes")]
        private readonly Dictionary<StatisticsAttributes, int> _attributes;

        public StatisticsData()
        {
            _attributes = CollectionFactory.GetDefaultEnumDictionary<StatisticsAttributes, int>();
        }

        [JsonConstructor]
        public StatisticsData(Dictionary<StatisticsAttributes, int> attributes)
        {
            CollectionFactory.AddMissingKeys(attributes);
            _attributes = attributes;
        }

        public override string ToString()
        {
            var dict = JsonReader.GetDictionary<StatisticsAttributes, string>(Resources.Dictionary_Statistics);
            return string.Join("\n\n", _attributes.Select(pair => $"{dict[pair.Key]}: {GetIntFormat(pair.Value, 999999999, "{0:#,0}")}"));
        }

        public int GetAttribute(StatisticsAttributes key)
        {
            return _attributes[key];
        }

        public void Update(StatisticsAttributes key, int value = 1)
        {
            _attributes[key] += value;
        }

        public void Save()
        {
            FileWriter.Save(this, GameDirectory.StatisticsFilePath);
        }

        private string GetIntFormat(int value, int maxValue, string format)
        {
            return value < maxValue ? string.Format(format, value) : string.Format($"{format}+", maxValue);
        }
    }
}
