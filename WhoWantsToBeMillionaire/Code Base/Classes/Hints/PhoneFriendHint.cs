using System.Collections.Generic;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class PhoneFriendHint : Hint
    {
        private readonly Dictionary<QuestionDifficulties, double> _probabilities;

        public PhoneFriendHint()
        {
            _probabilities = new Dictionary<QuestionDifficulties, double>
            {
                { QuestionDifficulties.Easy, 1.00 },
                { QuestionDifficulties.Normal, 0.80 },
                { QuestionDifficulties.Hard, 0.40 },
                { QuestionDifficulties.Final, 0.20 }
            };
        }

        public IEnumerable<string> GetFriendDialog(string sum)
        {
            return GetDialog(Resources.Dialog_Hint_PhoneFriend_Dialog, ("<SUM>", sum));
        }

        public string GetFriendAnswer(Question question)
        {
            string answer;
            byte[] dialogOptions;

            if (question.OptionsCount == 2 || GameRandom.CheckChance(_probabilities[question.Difficulty]))
            {
                answer = question.FullCorrect;
                dialogOptions = Resources.Dialog_Hint_PhoneFriend_Correct;
            }
            else
            {
                answer = question.GetFullOption(GetRandomWrongLetter(question));
                dialogOptions = Resources.Dialog_Hint_PhoneFriend_Incorrect;
            }

            var result = GetDialog(dialogOptions, ("<QUESTION>", question.Text), ("<ANSWER>", answer));
            return string.Join(string.Empty, result);
        }
    }
}
