﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    class Hint
    {
        private readonly Random random;

        public const int MaxCountAllowedHints = 4;

        public Hint()
        {
            random = new Random();
        }

        public Dictionary<Letter, float> PercentsAudience(Question question)
        {
            List<Letter> keys = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).Select(x => x.Key).OrderBy(k => random.Next()).ToList();
            List<float> percents = new List<float>();
            int sum = 101, randomPercent;

            for (int i = 0; i < keys.Count; i++)
            {
                randomPercent = random.Next(sum);
                percents.Add(randomPercent);
                sum -= randomPercent;
            }

            percents.Add(sum);
            percents = percents.OrderByDescending(x => x).ToList();

            if (random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(random.Next(keys.Count) + 1, question.Correct);

            return keys.Zip(percents, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        public string[] PhoneFriendDialog(string sum)
        {
            string[] dialogues = ResourceProcessing.GetString("Hint_PhoneFriend_Dialog.txt").Split(new string[] { "<SEPARATOR>" }, StringSplitOptions.None);

            string result = dialogues[random.Next(dialogues.Length)].Replace("<SUM>", sum);

            return result.Split(new string[] { "\n" }, StringSplitOptions.None);
        }

        public string PhoneFriendAnswer(Question question)
        {
            StringBuilder result = new StringBuilder();
            string fileName, answer;

            if (random.NextDouble() <= -0.07 * question.Number + 1.28 || question.CountOptions == 2)
            {
                answer = question.FullCorrect;
                fileName = "Hint_PhoneFriend_Correct.txt";
            }
            else
            {
                Letter wrong = question.Options.Where(x => x.Key != question.Correct).OrderBy(x => random.Next()).First().Key;
                answer = question.FullOption(wrong);
                fileName = "Hint_PhoneFriend_Incorrect.txt";
            }

            string[] dialogues = ResourceProcessing.GetString(fileName).Split(new string[] { "<SEPARATOR>" }, StringSplitOptions.None);
            result.Append(dialogues[random.Next(dialogues.Length)]);

            result.Replace("<ANSWER>", answer);
            result.Replace("<QUESTION>", question.Text);

            return result.ToString();
        }

        public string HostAnswer(Question question)
        {
            StringBuilder result = new StringBuilder();
            string fileName, answer;

            double b = question.CountOptions > 2 ? 1.25 : 1.5;
            bool isCorrect = random.NextDouble() <= -0.05 * question.Number + b;

            if (isCorrect)
            {
                answer = question.FullCorrect;
                fileName = "Hint_AskHost_Correct.txt";
            }
            else
            {
                Letter wrong = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).OrderBy(x => random.Next()).First().Key;
                answer = question.FullOption(wrong);
                fileName = "Hint_AskHost_Incorrect.txt";
            }

            string[] phrases = ResourceProcessing.GetString(fileName).Split(new string[] { "\n" }, StringSplitOptions.None);

            result.Append(isCorrect ? phrases[(question.Number - 1) / 5] : phrases[random.Next(phrases.Length)]);
            
            result.Replace("<ANSWER>", answer);
            result.Replace("<n>", "\n");

            return result.ToString();
        }

        public string Description(TypeHint type)
        {
            switch (type)
            {
                case TypeHint.FiftyFifty:
                    return "«50:50» — убирает два неверных варианта ответа.";
                case TypeHint.PhoneFriend:
                    return "«Звонок другу» — даёт возможность посоветоваться с другом по телефону.";
                case TypeHint.AskAudience:
                    return "«Помощь зала» — позволяет взять подсказку у зрителей зала.";
                case TypeHint.DoubleDip:
                    return "«Право на ошибку» — позволяет дать второй вариант ответа, если первый оказался неверным.";
                case TypeHint.SwitchQuestion:
                    return "«Замена вопроса» — меняет вопрос на другой.";
                case TypeHint.AskHost:
                    return "«Помощь ведущего» — позволяет взять подсказку у ведущего.";
            }

            return "Подсказка не найдена.";
        }
    }
}