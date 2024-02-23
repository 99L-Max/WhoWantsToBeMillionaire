using System;

namespace WhoWantsToBeMillionaire
{
    enum HostPhrases
    {
        Rules,
        SaveSums,
        CountHints,
        AboutRestrictionsHints,
        AboutTakingMoney,
        AskSaveSum,
        SaveSumSelected,
        GameStart,
        AskBeforeSwitchQuestion,
        SwitchQuestionCorrect,
        SwitchQuestionIncorrect,
        AskAfterTakingMoney,
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

                case HostPhrases.SaveSumSelected:
                    return $"{args[0]} рублей — несгораемая сумма!";

                case HostPhrases.GameStart:
                    return "И для Вас начинается игра «Кто хочет стать миллионером?»!!!";

                case HostPhrases.AskSaveSum:
                    return GetRandomPhrase("Host_AskSaveSum.txt");

                case HostPhrases.AskBeforeSwitchQuestion:
                    return GetRandomPhrase("Host_SwitchQuestion.txt");

                case HostPhrases.SwitchQuestionCorrect:
                    return GetRandomPhrase("Host_SwitchQuestion_CorrectAnswer.txt").Replace("<NUMBER>", args[0]);

                case HostPhrases.SwitchQuestionIncorrect:
                    return GetRandomPhrase("Host_SwitchQuestion_IncorrectAnswer.txt").Replace("<NUMBER>", args[0]);

                case HostPhrases.AskAfterTakingMoney:
                    return GetRandomPhrase("Host_AskAfterTakingMoney.txt");

                case HostPhrases.PlayerTakingMoney:
                    string answer = $"Игрок берёт деньги!\nВыигрыш нашего гостя сотавил {args[0]} рублей!";

                    if (args[0] == "0")
                        answer = "Забирать деньги на 1-м вопросе?.. Ваше право.\n" + answer;

                    return answer;
            }

            return string.Empty;
        }

        private string GetRandomPhrase(string fileName)
        {
            string[] phrases = ResourceProcessing.GetString(fileName).Split(new string[] { "\n" }, StringSplitOptions.None);

            return phrases[new Random().Next(phrases.Length)];
        }
    }
}
