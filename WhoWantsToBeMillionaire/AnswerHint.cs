using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string[] GetPhoneFriendDialog(string sum)
        {
            int numberDialog = random.Next(1, 4);

            string text = ResourceProcessing.GetString($"Hint_PhoneFriend_Dialog{numberDialog}.txt").Replace("<SUM>", sum);

            return text.Split(new string[] { "\n" }, StringSplitOptions.None);
        }

        public string GetPhoneFriendAnswer(Question question)
        {
            int numberDialog = random.Next(1, 4);

            StringBuilder result = new StringBuilder();

            if (random.NextDouble() <= -0.07 * question.Number + 1.28 || question.CountOptions == 2)
            {
                result.Append(ResourceProcessing.GetString($"Hint_PhoneFriend_Correct{numberDialog}.txt"));
                result.Replace("<CORRECT>", question.FullCorrect);
            }
            else
            {
                Letter wrong = question.Options.Where(x => x.Key != question.Correct).OrderBy(x => random.Next()).First().Key;

                result.Append(ResourceProcessing.GetString($"Hint_PhoneFriend_Incorrect{numberDialog}.txt"));
                result.Replace("<INCORRECT>", question.GetFullOption(wrong));
            }

            result.Replace("<QUESTION>", question.Text);

            return result.ToString();
        }

        public string GetAskHostAnswer(Question question)
        {
            StringBuilder result = new StringBuilder();

            double b = question.CountOptions > 2 ? 1.25 : 1.5;

            if (random.NextDouble() <= -0.05 * question.Number + b)
            {
                result.Append(ResourceProcessing.GetString($"Hint_AskHost_Correct.txt").Split(new string[] { "\n" }, StringSplitOptions.None)[(question.Number - 1) / 5]);
                result.Replace("<CORRECT>", question.FullCorrect);
            }
            else
            {
                Letter wrong = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).OrderBy(x => random.Next()).First().Key;

                result.Append(ResourceProcessing.GetString($"Hint_AskHost_Incorrect.txt").Split(new string[] { "\n" }, StringSplitOptions.None)[(question.Number - 1) / 5]);
                result.Replace("<INCORRECT>", question.GetFullOption(wrong));
            }

            result.Replace("<n>", "\n");

            return result.ToString();
        }

        public string GetPhraseSwitchQuestion()
        {
            string[] phrases = ResourceProcessing.GetString($"Hint_SwitchQuestion.txt").Split(new string[] { "\n" }, StringSplitOptions.None);

            return phrases[random.Next(phrases.Length)];
        }

        public string GetExplanationForSwitchQuestion(Question question, bool isCorrect)
        {
            string[] phrases = ResourceProcessing.GetString($"Hint_SwitchQuestion_{(isCorrect ? "Correct" : "Incorrect")}.txt").Split(new string[] { "\n" }, StringSplitOptions.None);
            string phrase = phrases[random.Next(phrases.Length)].Replace("<NUMBER>", question.Number.ToString());

            if (isCorrect)
                return $"{question.Explanation}\n{phrase}";
            else
                return $"{question.Explanation}\nПравильный ответ: {question.FullCorrect}, {phrase}";
        }
    }
}
