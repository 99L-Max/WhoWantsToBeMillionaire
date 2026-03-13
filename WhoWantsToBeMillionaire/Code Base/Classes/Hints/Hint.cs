using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    abstract class Hint
    {
        public const int MaxAllowedHintsCount = 4;

        protected IEnumerable<string> GetDialog(byte[] array, params (string, string)[] replace)
        {
            var dialogues = JsonReader.GetObject<string[][]>(array);

            IEnumerable<string> result = GameRandom.GetRandomElement(dialogues);

            foreach (var element in replace)
                result = result.Select(word => word.Replace(element.Item1, element.Item2));

            return result.Select(phrase => $"- {phrase}\n");
        }

        protected LetterOptions GetRandomWrongLetter(Question question)
        {
            var wrongKeys = question.Options.Where(option => option.Key != question.Correct && option.Value != string.Empty).Select(option => option.Key);
            return GameRandom.GetRandomElement(wrongKeys);
        }
    }
}
