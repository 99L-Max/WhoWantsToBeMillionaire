using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class Hint
    {
        public const int MaxCountAllowedHints = 4;

        private readonly Random random;

        public Hint() => random = new Random();

        public Question ReduceOptions(Question question)
        {
            var wrongKeys = question.Options.Keys.Where(k => k != question.Correct).ToList();
            var secondKey = wrongKeys[random.Next(wrongKeys.Count)];
            return new Question(question.Number, question.Index, new Letter[] { question.Correct, secondKey });
        }

        public Dictionary<Letter, int> PercentsAudience(Question question)
        {
            var keys = question.Options.Where(x => x.Value != string.Empty).Select(x => x.Key).OrderBy(k => random.Next()).ToList();
            var percents = new List<int>();
            var sum = 100;

            for (int i = 1; i < keys.Count; i++)
            {
                percents.Add(random.Next(sum));
                sum -= percents.Last();
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(x => x).ToList();

            keys.Remove(question.Correct);

            if (random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(random.Next(keys.Count) + 1, question.Correct);

            return keys.Zip(percents, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        private IEnumerable<string> GetDialog(string fileName, params (string, string)[] replace)
        {
            var jString = ResourceManager.GetDialog(fileName);
            var dialogues = JsonConvert.DeserializeObject<string[][]>(jString);

            IEnumerable<string> result = dialogues[random.Next(dialogues.Length)];

            foreach (var el in replace)
                result = result.Select(s => s.Replace(el.Item1, el.Item2));

            return result.Select(s => $"- {s}\n");
        }

        public IEnumerable<string> PhoneFriendDialog(string sum) =>
            GetDialog("Hint_PhoneFriend_Dialog.json", ("<SUM>", sum));

        public string PhoneFriendAnswer(Question question)
        {
            string fileName, answer;

            var probability = -0.07 * question.Number + 1.28;

            if (question.CountOptions == 2 || random.NextDouble() < probability)
            {
                answer = question.FullCorrect;
                fileName = "Hint_PhoneFriend_Correct.json";
            }
            else
            {
                var wrongKeys = question.Options.Where(x => x.Key != question.Correct).Select(x => x.Key);
                var key = wrongKeys.ElementAt(random.Next(wrongKeys.Count()));
                answer = question.FullOption(key);
                fileName = "Hint_PhoneFriend_Incorrect.json";
            }

            var result = GetDialog(fileName, ("<QUESTION>", question.Text), ("<ANSWER>", answer));
            return string.Join(string.Empty, result);
        }

        public string HostAnswer(Question question)
        {
            string fileName, answer;

            var b = question.CountOptions > 2 ? 1.25 : 1.5;
            var isCorrect = random.NextDouble() < -0.05 * question.Number + b;

            if (isCorrect)
            {
                answer = question.FullCorrect;
                fileName = "Hint_AskHost_Correct.json";
            }
            else
            {
                Letter wrong = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).OrderBy(x => random.Next()).First().Key;
                answer = question.FullOption(wrong);
                fileName = "Hint_AskHost_Incorrect.json";
            }

            var jString = ResourceManager.GetDialog(fileName);
            var phrases = JsonConvert.DeserializeObject<string[]>(jString);
            var result = phrases[isCorrect ? (int)question.Difficulty : random.Next(phrases.Length)];

            return result.Replace("<ANSWER>", answer);
        }
    }
}
