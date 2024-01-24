using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ContainerHints : ControlAnimation
    {
        private Hint[] hints;
        private Queue<Hint> hiddenHints;
        private int countUsedHints;

        public const int MaxCountAllowedHints = 4;

        public int CountHints => hints.Length;

        public int CountActiveHints => hints.Select(h => h.Enabled).Count();

        public bool AllHintsVisible => hiddenHints.Count == 0;

        public string StringCountActiveHints
        {
            get
            {
                int count = CountActiveHints;

                switch (count % 10)
                {
                    default:
                        return $"{count} подсказок";
                    case 1:
                        return $"{count} подсказка";
                    case 2:
                    case 3:
                    case 4:
                        return $"{count} подсказки";
                }
            }
        }

        public delegate void EventHintClick(TypeHint type);
        public event EventHintClick HintClick;

        public ContainerHints(Size size) : base(size) { }

        public void Reset()
        {
            Enabled = false;

            Controls.Clear();

            hints?.ToList().ForEach(b => b?.Dispose());

            countUsedHints = 0;

            int countColumns;
            TypeHint[] types;

            switch ((Mode)Settings.Default.Mode)
            {
                default:
                    types = new TypeHint[] { TypeHint.FiftyFifty, TypeHint.PhoneFriend, TypeHint.AskAudience };
                    countColumns = 3;
                    goto case Mode.TEST;
                    break;
                case Mode.Amateur:
                    types = new TypeHint[] { TypeHint.FiftyFifty, TypeHint.PhoneFriend, TypeHint.AskAudience, TypeHint.DoubleDip };
                    countColumns = 4;
                    break;
                case Mode.Advanced:
                    types = new TypeHint[] { TypeHint.AskHost, TypeHint.FiftyFifty, TypeHint.DoubleDip, TypeHint.PhoneFriend, TypeHint.SwitchQuestion };
                    countColumns = 3;
                    break;
                case Mode.TEST:
                    types = new TypeHint[] { TypeHint.AskAudience, TypeHint.AskHost, TypeHint.FiftyFifty, TypeHint.DoubleDip, TypeHint.PhoneFriend, TypeHint.SwitchQuestion };
                    countColumns = 3;
                    break;
            }

            int width = (int)(0.9f * Width / countColumns);
            Size sizeHint = new Size(width, (int)(0.63f * width));

            hints = types.Select(t => new Hint(t)).ToArray();

            foreach (var hint in hints)
                hint.Click += OnHintClick;

            hiddenHints = new Queue<Hint>(hints);

            SetLocationsHints(new Queue<Hint>(hints), sizeHint, countColumns);
        }

        private void OnHintClick(object sender, EventArgs e)
        {
            Hint hint = sender as Hint;

            Enabled = hint.Type == TypeHint.FiftyFifty;
            countUsedHints++;

            hint.Enabled = false;
            hint.Click -= OnHintClick;

            if (countUsedHints >= MaxCountAllowedHints)
                foreach (var h in hints.Where(x => x.Enabled))
                {
                    h.Lock();
                    h.Click -= OnHintClick;
                }

            HintClick.Invoke(hint.Type);
        }

        private void SetLocationsHints(Queue<Hint> hints, Size sizeHint, int countColumns)
        {
            int countRows = (int)Math.Ceiling((float)hints.Count / countColumns);

            int x0 = (Size.Width - sizeHint.Width * countColumns) / 2;
            int y0 = (Size.Height - sizeHint.Height * countRows) / 2;

            int y;

            Queue<Hint> queue = new Queue<Hint>();
            Hint hint;

            for (int row = 0; row < countRows; row++)
            {
                if (hints.Count >= countColumns)
                {
                    for (int i = 0; i < countColumns; i++)
                        queue.Enqueue(hints.Dequeue());
                }
                else
                {
                    queue = hints;
                    x0 = (Size.Width - sizeHint.Width * queue.Count) / 2;
                }

                y = y0 + row * sizeHint.Height;
                int count = queue.Count;

                for (int i = 0; i < count; i++)
                {
                    hint = queue.Dequeue();

                    hint.Location = new Point(x0 + i * sizeHint.Width, y);
                    hint.Size = sizeHint;

                    Controls.Add(hint);
                }
            }
        }

        public string GetDescriptionHint()
        {
            if (hiddenHints?.Count > 0)
                switch (hiddenHints.Peek().Type)
                {
                    case TypeHint.FiftyFifty:
                        return "«50:50» — убирает два неверных варианта ответа.";
                    case TypeHint.PhoneFriend:
                        return "«Звонок другу» — даёт возможность посоветоваться с другом по телефону.";
                    case TypeHint.AskAudience:
                        return "«Помощь зала» — позволяет взять подсказку у зрителей зала.";
                    case TypeHint.DoubleDip:
                        return "«Право на ошибку» — позволяет дать второй вариант ответа, если первый оказался неверным.";
                    case TypeHint.SwitchQuestion:
                        return "«Замена вопроса» — меняет вопрос на другой.";
                    case TypeHint.AskHost:
                        return "«Помощь ведущего» — позволяет взять подсказку у ведущего.";
                }

            return "Подсказка не найдена.";
        }

        public void ShowHint()
        {
            hiddenHints.Dequeue().Show();
        }

        public void ShowAllHints()
        {
            while (hiddenHints?.Count > 0)
                hiddenHints.Dequeue().Show();
        }
    }
}
