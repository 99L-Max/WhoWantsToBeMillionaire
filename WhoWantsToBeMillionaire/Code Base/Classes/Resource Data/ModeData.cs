using Newtonsoft.Json;
using System;

namespace WhoWantsToBeMillionaire
{
    class ModeData
    {
        [JsonConstructor]
        public ModeData(string name, int[] indexesSavingSums, HintTypes[] hints)
        {
            Name = name;
            IndexesSavingSums = indexesSavingSums;
            Hints = hints;
        }

        public string Name { get; }
        public int[] IndexesSavingSums { get; }
        public HintTypes[] Hints { get; }

        public string Description =>
            $"Подсказок: {Hints.Length}\n\n" +
            $"Несгораемых сумм: {Math.Max(IndexesSavingSums.Length, 1)}";
    }
}
