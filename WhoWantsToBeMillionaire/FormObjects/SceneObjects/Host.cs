﻿using System;

namespace WhoWantsToBeMillionaire
{
    enum HostPhrases
    {
        Rules,
        SaveSums,
        CountHints,
        AboutRestrictionsHints,
        AboutTakingMoney,
        AboutFinalQuestion,
        AskSaveSum,
        SaveSumSelected,
        GameStart,
        SwitchQuestion_AskAnswer,
        SwitchQuestion_CorrectAnswer,
        SwitchQuestion_IncorrectAnswer,
        TakingMoney_ClarifyDecision,
        TakingMoney_AskAnswer,
        TakingMoney_CorrectAnswer,
        TakingMoney_IncorrectAnswer,
        PlayerTakingMoney
    }

    class Host
    {
        public string Say(HostPhrases phrase, params string[] args)
        {
            switch (phrase)
            {
                case HostPhrases.Rules:
                    return
                        $"Вам необходимо правильно ответить на {args[0]} вопросов из различных областей знаний. " +
                        $"Каждый вопрос имеет 4 варианта ответа, из которых только один является верным.";

                case HostPhrases.SaveSums:
                    return $"Несгораемые суммы: {args[0]}.";

                case HostPhrases.CountHints:
                    return $"У Вас есть {args[0]}.";

                case HostPhrases.AboutRestrictionsHints:
                    return $"Но использовать можно только {args[0]} из них.";

                case HostPhrases.AboutTakingMoney:
                    return "До тех пор, пока Вы не дали ответ, можете забрать выигранные деньги.";

                case HostPhrases.AboutFinalQuestion:
                    return $"А теперь мы с Вами подошли к кульминационному моменту. " +
                           $"Лишь немногие достигали наивысшую планку игры «Кто хочет стать миллионером?», " +
                           $"а правильно отвечали на последний вопрос единицы. Последний рубеж!" +
                           $"\n{args[0]}-й вопрос на {args[1]} рублей.";

                case HostPhrases.SaveSumSelected:
                    return $"{args[0]} рублей — несгораемая сумма!";

                case HostPhrases.GameStart:
                    return "И для Вас начинается игра «Кто хочет стать миллионером?»!!!";

                case HostPhrases.AskSaveSum:
                    return RandomPhrase("Host_AskSaveSum.txt");

                case HostPhrases.SwitchQuestion_AskAnswer:
                    return RandomPhrase("Host_SwitchQuestion_AskAnswer.txt");

                case HostPhrases.SwitchQuestion_CorrectAnswer:
                    return RandomPhrase("Host_SwitchQuestion_CorrectAnswer.txt").Replace("<NUMBER>", args[0]);

                case HostPhrases.SwitchQuestion_IncorrectAnswer:
                    return RandomPhrase("Host_SwitchQuestion_IncorrectAnswer.txt").Replace("<NUMBER>", args[0]);

                case HostPhrases.TakingMoney_ClarifyDecision:
                    return "Вы хотите забрать деньги?";

                case HostPhrases.TakingMoney_AskAnswer:
                    return RandomPhrase("Host_TakingMoney_AskAnswer.txt");

                case HostPhrases.TakingMoney_CorrectAnswer:
                    return RandomPhrase("Host_TakingMoney_CorrectAnswer.txt").Replace("<SUM>", args[0]);

                case HostPhrases.TakingMoney_IncorrectAnswer:
                    return RandomPhrase("Host_TakingMoney_IncorrectAnswer.txt");

                case HostPhrases.PlayerTakingMoney:
                    string answer = $"Игрок берёт деньги!\nВыигрыш нашего гостя сотавил {args[0]} рублей!";

                    if (args[0] == "0")
                        answer = "Забирать деньги на 1-м вопросе?.. Ваше право.\n" + answer;

                    return answer;
            }

            return string.Empty;
        }

        private string RandomPhrase(string fileName)
        {
            string[] phrases = ResourceManager.GetString(fileName).Split(new string[] { "\n" }, StringSplitOptions.None);

            return phrases[new Random().Next(phrases.Length)];
        }
    }
}