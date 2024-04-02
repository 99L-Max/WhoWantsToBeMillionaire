using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Hint
    {
        public const int MaxCountAllowedHints = 4;

        private readonly Random _random = new Random();

        private Letter GetWrongLetter(Question question)
        {
            var wrongKeys = question.Options.Where(x => x.Key != question.Correct).Select(x => x.Key);
            return wrongKeys.ElementAt(_random.Next(wrongKeys.Count()));
        }

        public Question ReduceOptions(Question question) => 
            new Question(question.Number, question.Index, new Letter[] { question.Correct, GetWrongLetter(question) });

        public Dictionary<Letter, int> PercentsAudience(Question question)
        {
            var keys = question.Options.Where(x => x.Value != string.Empty).Select(x => x.Key).OrderBy(k => _random.Next()).ToList();
            var percents = new List<int>();
            var sum = 100;

            for (int i = 1; i < keys.Count; i++)
            {
                percents.Add(_random.Next(sum));
                sum -= percents.Last();
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(x => x).ToList();

            keys.Remove(question.Correct);

            if (_random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(_random.Next(keys.Count) + 1, question.Correct);

            return keys.Zip(percents, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        private IEnumerable<string> GetDialog(byte[] array, params (string, string)[] replace)
        {
            var dialogues = JsonManager.GetObject<string[][]>(array);

            IEnumerable<string> result = dialogues[_random.Next(dialogues.Length)];

            foreach (var el in replace)
                result = result.Select(s => s.Replace(el.Item1, el.Item2));

            return result.Select(s => $"- {s}\n");
        }

        public IEnumerable<string> PhoneFriendDialog(string sum) =>
            GetDialog(Resources.Dialog_Hint_PhoneFriend_Dialog, ("<SUM>", sum));

        public string PhoneFriendAnswer(Question question)
        {
            string answer;
            byte[] array;

            var probability = -0.07 * question.Number + 1.28;

            if (question.CountOptions == 2 || _random.NextDouble() < probability)
            {
                answer = question.FullCorrect;
                array = Resources.Dialog_Hint_PhoneFriend_Correct;
            }
            else
            {
                answer = question.FullOption(GetWrongLetter(question));
                array = Resources.Dialog_Hint_PhoneFriend_Incorrect;
            }

            var result = GetDialog(array, ("<QUESTION>", question.Text), ("<ANSWER>", answer));
            return string.Join(string.Empty, result);
        }

        public string HostAnswer(Question question)
        {
            string answer;
            byte[] array;

            var b = question.CountOptions > 2 ? 1.25 : 1.5;
            var isCorrect = _random.NextDouble() < -0.05 * question.Number + b;

            if (isCorrect)
            {
                answer = question.FullCorrect;
                array = Resources.Dialog_Hint_AskHost_Correct;
            }
            else
            {
                answer = question.FullOption(GetWrongLetter(question));
                array = Resources.Dialog_Hint_AskHost_Incorrect;
            }

            var phrases = JsonManager.GetObject<string[]>(array);
            var result = phrases[isCorrect ? (int)question.Difficulty : _random.Next(phrases.Length)];

            return result.Replace("<ANSWER>", answer);
        }
    }
}
