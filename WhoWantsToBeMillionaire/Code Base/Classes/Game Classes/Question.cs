using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Question
    {
        public const int MinNumber = 1;
        public const int MaxNumber = 15;

        private readonly Dictionary<LetterOptions, string> _options;

        public Question(int number) : this(number, GetRandomVersion(number)) { }

        public Question(int number, int version)
        {
            var array = (byte[])Resources.ResourceManager.GetObject($"Q{number:d2}V{version:d2}");
            var letters = CollectionFactory.GetEnum<LetterOptions>();
            var data = JsonReader.GetObject<QuestionData>(array);

            GameRandom.Shuffle(ref letters);
            _options = CollectionFactory.JoinToDictionary(letters, data.Options);

            Number = number;
            Version = version;
            Text = data.Question;
            Explanation = data.Explanation;
            Difficulty = GetDifficulty(number);
            Options = new ReadOnlyDictionary<LetterOptions, string>(_options);
            Correct = Options.First().Key;
        }

        public int Number { get; }
        public int Version { get; }
        public string Text { get; }
        public string Explanation { get; }
        public LetterOptions Correct { get; }
        public QuestionDifficulties Difficulty { get; }
        public ReadOnlyDictionary<LetterOptions, string> Options { get; private set; }

        public string FullCorrect => GetFullOption(Correct);

        public int OptionsCount
        {
            get => Options.Values.Count(option => option != string.Empty);
            set => SetOptionsCount(value);
        }

        public static QuestionDifficulties GetDifficulty(int number)
        {
            return number >= MaxNumber ? QuestionDifficulties.Final : (QuestionDifficulties)((number - 1) / 5);
        }

        public static int GetRandomVersion(int number)
        {
            return GameRandom.Next(35 - (number - 1) / 3 * 5) + 1;
        }

        public string GetFullOption(LetterOptions key)
        {
            return $"«{key}: {Options[key]}»";
        }

        private void SetOptionsCount(int count)
        {
            count = GameMath.Clamp(count, 1, _options.Count);

            var options = new Dictionary<LetterOptions, string>(_options);

            foreach (var key in options.Keys.Skip(count).ToArray())
            {
                options[key] = string.Empty;
            }

            Options = new ReadOnlyDictionary<LetterOptions, string>(options);
        }
    }
}
