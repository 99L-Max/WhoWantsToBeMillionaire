using Newtonsoft.Json;
using System;

namespace WhoWantsToBeMillionaire
{
    class ModeData
    {
        [JsonConstructor]
        public ModeData(string name, int[] numbersQuestionsSavingSums, HintTypes[] hints)
        {
            Name = name;
            NumbersQuestionsSavingSums = numbersQuestionsSavingSums;
            Hints = hints;
        }

        public string Name { get; }
        public int[] NumbersQuestionsSavingSums { get; }
        public HintTypes[] Hints { get; }

        public string Description =>
            $"Подсказок: {Hints.Length}\n\n" +
            $"Несгораемых сумм: {Math.Max(NumbersQuestionsSavingSums.Length, 1)}";
    }
}
