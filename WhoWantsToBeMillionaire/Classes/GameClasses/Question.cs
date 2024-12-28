using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum LetterOption { A, B, C, D }

    enum DifficultyQuestion { Easy, Normal, Hard, Final }

    class Question
    {
        public const int MaxNumber = 15;

        private static readonly Random s_random;

        public readonly int Seed;
        public readonly int Number;
        public readonly int Version;
        public readonly string Text;
        public readonly string Explanation;
        public readonly LetterOption Correct;
        public readonly DifficultyQuestion Difficulty;
        public readonly ReadOnlyDictionary<LetterOption, string> Options;

        static Question() =>
            s_random = new Random();

        public Question(int number) : this(number, RandomVersion(number), s_random.Next(), 4) { }

        public Question(int number, int version) : this(number, version, s_random.Next(), 4) { }

        public Question(int number, int version, int seed, int countOptions)
        {
            var array = (byte[])Resources.ResourceManager.GetObject($"Q{number:d2}V{version:d2}");
            var jObj = JsonManager.GetObject(array);
            var random = new Random(seed);
            var letters = LettersOption.OrderBy(x => random.Next()).ToArray();
            var options = JsonConvert.DeserializeObject<string[]>(jObj["Options"].ToString());
            var dict = new Dictionary<LetterOption, string>();

            for (int i = 0; i < letters.Length; i++)
                dict.Add(letters[i], i < countOptions ? options[i] : string.Empty);

            Seed = seed;
            Number = number;
            Version = version;
            Difficulty = Number == MaxNumber ? DifficultyQuestion.Final : (DifficultyQuestion)((Number - 1) / 5);
            Text = jObj["Question"].Value<string>();
            Explanation = jObj["Explanation"].Value<string>();
            Options = new ReadOnlyDictionary<LetterOption, string>(dict);
            Correct = letters[0];
        }

        public string FullCorrect =>
            FullOption(Correct);

        public int CountOptions =>
            Options.Values.Where(x => x != string.Empty).Count();

        public static IEnumerable<LetterOption> LettersOption =>
            Enum.GetValues(typeof(LetterOption)).Cast<LetterOption>();

        public static int RandomVersion(int number) =>
            s_random.Next(35 - (number - 1) / 3 * 5) + 1;

        public string FullOption(LetterOption key) =>
            $"«{key}: {Options[key]}»";
    }
}
