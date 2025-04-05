using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Host
    {
        private readonly JObject _phrases;
        private readonly Random _random;

        public Host()
        {
            _phrases = JsonManager.GetObject(Resources.Dialog_Host);
            _random = new Random();
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
                result.Append(array[_random.Next(array.Length)]);
            }

            for (int i = 0; i < args.Length; i++)
                result.Replace($"<ARG_{i}>", args[i]);

            return result.ToString();
        }
    }
}
