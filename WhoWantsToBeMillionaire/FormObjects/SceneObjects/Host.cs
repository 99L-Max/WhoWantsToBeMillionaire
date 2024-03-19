using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

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
        PlayerTakingMoney,
        PlayerTakingMoney_Zero
    }

    class Host
    {
        private readonly JObject phrases;
        private readonly Random random;

        public Host()
        {
            using (Stream stream = ResourceManager.GetStream("Host.json", TypeResource.Dialogues))
            using (StreamReader reader = new StreamReader(stream))
            {
                phrases = JObject.Parse(reader.ReadToEnd());
                random = new Random();
            }
        }

        public string Say(HostPhrases phrase, params string[] args)
        {
            JToken token = phrases[phrase.ToString()];
            StringBuilder result = new StringBuilder();

            if (token.Type == JTokenType.String)
            {
                result.Append(token.Value<string>());
            }
            else
            {
                var array = JsonConvert.DeserializeObject<string[]>(token.ToString());
                result.Append(array[random.Next(array.Length)]);
            }

            for (int i = 0; i < args.Length; i++)
                result.Replace($"<ARG_{i}>", args[i]);

            return result.ToString();
        }
    }
}
