using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class Question
    {
        public readonly int Number;
        public readonly int Index;
        public readonly string Text;
        public readonly string Explanation;
        public readonly char Correct;
        public readonly ReadOnlyDictionary<char, string> Options;

        public string FullCorrect => GetFullOption(Correct);

        public int CountOptions => Options.Values.Where(x => x != string.Empty).Count();

        public Question(int number, int index)
        {
            Number = number;
            Index = index;

            //using (Stream stream = ResourceProcessing.GetStream($"Q{number:d2}V{index:d2}.json", TypeResource.Questions))
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    JObject jObj = JObject.Parse(reader.ReadToEnd());
            //
            //    var values = JsonConvert.DeserializeObject<Dictionary<char, string>>(jObj["Options"].ToString());
            //
            //    Text = jObj["Question"].Value<string>();
            //    Explanation = jObj["Explanation"].Value<string>();
            //    Correct = jObj["Correct"].Value<char>();
            //    Options = new ReadOnlyDictionary<char, string>(values);
            //}

            //ЗАГЛУШКА
            Dictionary<char, string> op = new Dictionary<char, string>() { { 'A', "Вариант" }, { 'B', "Вариант" }, { 'C', "Вариант" }, { 'D', "Вариант" } };

            Text = $"Вопрос?\n №{number}";
            Explanation = "Поясняю";
            Correct = 'A';
            Options = new ReadOnlyDictionary<char, string>(op);
        }

        public Question(int number, int index, string text, Dictionary<char, string> options, char correct, string explanation)
        {
            Number = number;
            Index = index;

            Text = text;
            Options = new ReadOnlyDictionary<char, string>(options);
            Correct = correct;
            Explanation = explanation;
        }

        public static int RandomIndex(int number)
        {
            return new Random().Next(35 - (number - 1) / 3 * 5) + 1;
        }

        public string GetFullOption(char key) => $"«{key}: {Options[key]}»";
    }
}
