using System;

namespace WhoWantsToBeMillionaire
{
    enum HostPhrases
    {
        Rules
    }

    class Host
    {
        private readonly Random random;

        public Host()
        {
            random = new Random();
        }

        public string SayPlayerTakesMoney(int prize)
        {
            string phrase = $"Игрок берёт деньги!\nВыигрыш нашего гостя сотавил {prize} рублей!";

            if (prize == 0)
                phrase = "Забирать деньги на 1-м вопросе?.. Ваше право.\n" + phrase;

            return phrase;
        }

        public string AskAfterTakingMoney()
        {
            string[] phrases = ResourceProcessing.GetString($"PlayerTakesMoney.txt").Split(new string[] { "\n" }, StringSplitOptions.None);

            return phrases[random.Next(phrases.Length)];
        }
    }
}
