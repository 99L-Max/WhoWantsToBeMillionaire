using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Host
    {
        private readonly JObject _phrases;

        public Host()
        {
            _phrases = JsonReader.GetObject(Resources.Dialog_Host);
        }

        public string Say(HostPhrases phrase, params string[] args)
        {
            var token = _phrases[phrase.ToString()];
            var result = new StringBuilder();

            if (token.Type == JTokenType.String)
            {
                result.Append(token.Value<string>());
            }
            else
            {
                var array = JsonConvert.DeserializeObject<string[]>(token.ToString());
                result.Append(GameRandom.GetRandomElement(array));
            }

            for (int i = 0; i < args.Length; i++)
            { 
                result.Replace($"<ARG_{i}>", args[i]);
            }

            return result.ToString();
        }
    }
}
