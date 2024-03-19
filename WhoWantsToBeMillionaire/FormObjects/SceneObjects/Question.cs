using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum Letter { A, B, C, D }

    enum DifficultyQuestion
    {
        Easy,
        Normal,
        Difficult,
        Final
    }

    class Question
    {
        public const int MaxNumber = 15;

        public readonly int Number;
        public readonly int Index;
        public readonly DifficultyQuestion Difficulty;
        public readonly string Text;
        public readonly string Explanation;
        public readonly Letter Correct;
        public readonly ReadOnlyDictionary<Letter, string> Options;

        public string FullCorrect => FullOption(Correct);

        public int CountOptions => Options.Values.Where(x => x != string.Empty).Count();

        public static IEnumerable<Letter> Letters => Enum.GetValues(typeof(Letter)).Cast<Letter>();

        public Question(int number) : this(number, RandomIndex(number)) { }

        public Question(int number, int index) : this(number, index, Letters) { }

        public Question(int number, int index, IEnumerable<Letter> letters)
        {
            Number = number;
            Index = index;
            Difficulty = Number == MaxNumber ? DifficultyQuestion.Final : (DifficultyQuestion)((Number - 1) / 5);

            using (Stream stream = ResourceManager.GetStream($"Q{number:d2}V{index:d2}.json", TypeResource.Questions))
            using (StreamReader reader = new StreamReader(stream))
            {
                JObject jObj = JObject.Parse(reader.ReadToEnd());

                Text = jObj["Question"].Value<string>();
                Explanation = jObj["Explanation"].Value<string>();
                Correct = (Letter)Enum.Parse(typeof(Letter), jObj["Correct"].Value<string>());

                if (!letters.Contains(Correct))
                    letters.Append(Correct);

                var options = JsonConvert.DeserializeObject<Dictionary<Letter, string>>(jObj["Options"].ToString());

                foreach (var key in options.Keys.ToArray())
                    if (!letters.Contains(key))
                        options[key] = string.Empty;

                Options = new ReadOnlyDictionary<Letter, string>(options);
            }
        }

        public static int RandomIndex(int number) =>
            new Random().Next(35 - (number - 1) / 3 * 5) + 1;

        public string FullOption(Letter key) =>
            $"«{key}: {Options[key]}»";
    }
}
