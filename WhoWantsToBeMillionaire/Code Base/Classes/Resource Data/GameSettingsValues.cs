using Newtonsoft.Json;
using System.Collections.Generic;

namespace WhoWantsToBeMillionaire
{
    class GameSettingsValues
    {
        [JsonConstructor]
        public GameSettingsValues(string name, float defaultValue, Dictionary<float, string> comboBoxItems)
        {
            Name = name;
            DefaultValue = defaultValue;
            ComboBoxItems = comboBoxItems;
        }

        public string Name { get; }
        public float DefaultValue { get; }
        public Dictionary<float, string> ComboBoxItems { get; }
    }
}
