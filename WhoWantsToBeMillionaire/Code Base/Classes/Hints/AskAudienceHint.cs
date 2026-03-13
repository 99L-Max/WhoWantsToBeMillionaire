using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class AskAudienceHint : Hint
    {
        private readonly Dictionary<QuestionDifficulties, double> _probabilities;

        public AskAudienceHint()
        {
            _probabilities = new Dictionary<QuestionDifficulties, double>
            {
                { QuestionDifficulties.Easy, 1.00 },
                { QuestionDifficulties.Normal, 0.70 },
                { QuestionDifficulties.Hard, 0.30 },
                { QuestionDifficulties.Final, 0.10 }
            };
        }

        public Dictionary<LetterOptions, int> GetPercentagesAudience(Question question)
        {
            var keys = question.Options.Where(option => option.Value != string.Empty).Select(option => option.Key).OrderBy(_ => GameRandom.Next()).ToList();
            var percents = new List<int>();
            var sum = GameConst.MaxPercent;

            for (int i = 1; i < keys.Count; i++)
            {
                percents.Add(GameRandom.Next(sum));
                sum -= percents.Last();
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(_ => _).ToList();

            keys.Remove(question.Correct);

            if (keys.Count == 2 || GameRandom.CheckChance(_probabilities[question.Difficulty]))
                keys.Insert(0, question.Correct);
            else
                keys.Insert(GameRandom.Next(keys.Count) + 1, question.Correct);

            return CollectionFactory.JoinToDictionary(keys, percents);
        }
    }
}
