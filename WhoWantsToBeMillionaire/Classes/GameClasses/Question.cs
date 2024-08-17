using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum Letter { A, B, C, D }

    enum DifficultyQuestion { Easy, Normal, Hard, Final }

    class Question
    {
        public const int MaxNumber = 15;

        private static readonly Random s_random;

        public readonly int Seed;
        public readonly int Number;
        public readonly int Index;
        public readonly string Text;
        public readonly string Explanation;
        public readonly Letter Correct;
        public readonly DifficultyQuestion Difficulty;
        public readonly ReadOnlyDictionary<Letter, string> Options;

        static Question() =>
            s_random = new Random();

        public Question(int number) : this(number, RandomIndex(number), s_random.Next(), 4) { }

        public Question(int number, int index) : this(number, index, s_random.Next(), 4) { }

        public Question(int number, int index, int seed, int countOptions)
        {
            var array = (byte[])Resources.ResourceManager.GetObject($"Q{number:d2}V{index:d2}");
            var jObj = JsonManager.GetObject(array);
            var random = new Random(seed);
            var letters = Letters.OrderBy(x => random.Next()).ToArray();
            var options = JsonConvert.DeserializeObject<string[]>(jObj["Options"].ToString());
            var dict = new Dictionary<Letter, string>();

            for (int i = 0; i < letters.Length; i++)
                dict.Add(letters[i], i < countOptions ? options[i] : string.Empty);

            Seed = seed;
            Number = number;
            Index = index;
            Difficulty = Number == MaxNumber ? DifficultyQuestion.Final : (DifficultyQuestion)((Number - 1) / 5);
            Text = jObj["Question"].Value<string>();
            Explanation = jObj["Explanation"].Value<string>();
            Options = new ReadOnlyDictionary<Letter, string>(dict);
            Correct = letters[0];
        }

        public string FullCorrect =>
            GetFullOption(Correct);

        public int CountOptions =>
            Options.Values.Where(x => x != string.Empty).Count();

        public static IEnumerable<Letter> Letters =>
            Enum.GetValues(typeof(Letter)).Cast<Letter>();

        public static int RandomIndex(int number) =>
            s_random.Next(35 - (number - 1) / 3 * 5) + 1;

        public string GetFullOption(Letter key) =>
            $"«{key}: {Options[key]}»";
    }
}
