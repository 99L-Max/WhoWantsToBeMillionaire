using System.Collections.Generic;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class AskHostHint : Hint
    {
        private readonly Dictionary<QuestionDifficulties, double> _probabilities;

        public AskHostHint()
        {
            _probabilities = new Dictionary<QuestionDifficulties, double>
            {
                { QuestionDifficulties.Easy, 1.00 },
                { QuestionDifficulties.Normal, 1.00 },
                { QuestionDifficulties.Hard, 0.75 },
                { QuestionDifficulties.Final, 0.50 }
            };
        }

        public string GetHostAnswer(Question question)
        {
            string answer;
            byte[] array;

            var isCorrect = GameRandom.CheckChance(_probabilities[question.Difficulty]);

            if (isCorrect)
            {
                answer = question.FullCorrect;
                array = Resources.Dialog_Hint_AskHost_Correct;
            }
            else
            {
                answer = question.GetFullOption(GetRandomWrongLetter(question));
                array = Resources.Dialog_Hint_AskHost_Incorrect;
            }

            var phrases = JsonReader.GetObject<string[]>(array);
            var result = phrases[isCorrect ? (int)question.Difficulty : GameRandom.Next(phrases.Length)];

            return result.Replace("<ANSWER>", answer);
        }
    }
}
