using System;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class AnswerHint
    {
        private readonly Random random;

        public AnswerHint()
        {
            random = new Random();
        }

        public Dictionary<Letter, float> GetPersents(Question question)
        {
            List<Letter> keys = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).Select(x => x.Key).OrderBy(k => random.Next()).ToList();
            List<float> percents = new List<float>();
            int sum = 101, randomPersent;

            for (int i = 0; i < keys.Count; i++)
            {
                randomPersent = random.Next(sum);
                percents.Add(randomPersent);
                sum -= randomPersent;
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(x => x).ToList();

            if (random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(random.Next(keys.Count) + 1, question.Correct);

            return keys.Zip(percents, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }
    }
}
