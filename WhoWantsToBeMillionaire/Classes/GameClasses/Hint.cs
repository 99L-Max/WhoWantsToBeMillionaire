using System;
using System.Collections.Generic;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class Hint
    {
        public const int MaxCountAllowedHints = 4;

        private static readonly Random s_random;

        static Hint() =>
            s_random = new Random();

        private static LetterOption GetWrongLetter(Question question)
        {
            var wrongKeys = question.Options.Where(x => x.Key != question.Correct).Select(x => x.Key);
            return wrongKeys.ElementAt(s_random.Next(wrongKeys.Count()));
        }

        public static Dictionary<LetterOption, int> GetPercentagesAudience(Question question)
        {
            var keys = question.Options.Where(x => x.Value != string.Empty).Select(x => x.Key).OrderBy(k => s_random.Next()).ToList();
            var percents = new List<int>();
            var sum = 100;

            for (int i = 1; i < keys.Count; i++)
            {
                percents.Add(s_random.Next(sum));
                sum -= percents.Last();
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(x => x).ToList();

            keys.Remove(question.Correct);

            if (s_random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(s_random.Next(keys.Count) + 1, question.Correct);

            return keys.Zip(percents, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        private static IEnumerable<string> GetDialog(byte[] array, params (string, string)[] replace)
        {
            var dialogues = JsonManager.GetObject<string[][]>(array);

            IEnumerable<string> result = dialogues[s_random.Next(dialogues.Length)];

            foreach (var el in replace)
                result = result.Select(s => s.Replace(el.Item1, el.Item2));

            return result.Select(s => $"- {s}\n");
        }

        public static IEnumerable<string> GetFriendDialog(string sum) =>
            GetDialog(Resources.Dialog_Hint_PhoneFriend_Dialog, ("<SUM>", sum));

        public static string GetFriendAnswer(Question question)
        {
            string answer;
            byte[] array;

            var probability = -0.07 * question.Number + 1.28;

            if (question.CountOptions == 2 || s_random.NextDouble() < probability)
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

        public static string GetHostAnswer(Question question)
        {
            string answer;
            byte[] array;

            var b = question.CountOptions > 2 ? 1.25 : 1.5;
            var probability = -0.05 * question.Number + b;
            var isCorrect = s_random.NextDouble() < probability;

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
            var result = phrases[isCorrect ? (int)question.Difficulty : s_random.Next(phrases.Length)];

            return result.Replace("<ANSWER>", answer);
        }
    }
}
