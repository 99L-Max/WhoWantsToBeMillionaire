using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum Letter { A, B, C, D }

    class Question
    {
        public readonly int Number;
        public readonly int Index;
        public readonly string Text;
        public readonly string Explanation;
        public readonly Letter Correct;
        public readonly ReadOnlyDictionary<Letter, string> Options;

        public string FullCorrect => GetFullOption(Correct);

        public int CountOptions => Options.Values.Where(x => x != string.Empty).Count();

        public Question(int number, int index)
        {
            Number = number;
            Index = index;

            using (Stream stream = ResourceProcessing.GetStream($"Q{number:d2}V{index:d2}.json", TypeResource.Questions))
            using (StreamReader reader = new StreamReader(stream))
            {
                JObject jObj = JObject.Parse(reader.ReadToEnd());

                var options = JsonConvert.DeserializeObject<Dictionary<Letter, string>>(jObj["Options"].ToString());

                Text = jObj["Question"].Value<string>();
                Explanation = jObj["Explanation"].Value<string>();
                Options = FormatOptions(options);
                Correct = (Letter)Enum.Parse(typeof(Letter), jObj["Correct"].Value<string>());
            }
        }

        public Question(int number, int index, string text, Dictionary<Letter, string> options, Letter correct, string explanation)
        {
            Number = number;
            Index = index;

            Text = text;
            Options = FormatOptions(options);
            Correct = correct;
            Explanation = explanation;
        }

        private ReadOnlyDictionary<Letter, string> FormatOptions(Dictionary<Letter, string> dict)
        {
            foreach (var key in Enum.GetValues(typeof(Letter)).Cast<Letter>())
                if (!dict.Keys.Contains(key))
                    dict.Add(key, string.Empty);

            return new ReadOnlyDictionary<Letter, string>(dict.OrderBy(x => x.Key).ToDictionary(k => k.Key, v => v.Value));
        }

        public static int RandomIndex(int number)
        {
            return new Random().Next(35 - (number - 1) / 3 * 5) + 1;
        }

        public string GetFullOption(Letter key) => $"«{key}: {Options[key]}»";
    }
}
