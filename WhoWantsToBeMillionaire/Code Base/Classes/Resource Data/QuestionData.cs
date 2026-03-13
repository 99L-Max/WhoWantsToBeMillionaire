using Newtonsoft.Json;

namespace WhoWantsToBeMillionaire
{
    class QuestionData
    {
        [JsonConstructor]
        public QuestionData(string question, string[] options, string explanation)
        {
            Question = question;
            Options = options;
            Explanation = explanation;
        }

        public string Question { get; }
        public string[] Options { get; }
        public string Explanation { get; }
    }
}
