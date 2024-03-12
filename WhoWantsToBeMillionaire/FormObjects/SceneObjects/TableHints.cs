using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class TableHints : GameContol, IReset
    {
        private ButtonHint[] hints;
        private Queue<ButtonHint> hiddenHints;
        private int countUsedHints;

        public int CountHints => hints.Length;

        public int CountActiveHints => hints.Select(h => h.Enabled).Count();

        public TypeHint PeekHiddenHint => hiddenHints.Peek().Type;

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

        public TableHints(int width, int height) : base(width, height) { }

        public void Reset(Mode? mode = null)
        {
            Visible = Enabled = false;

            Controls.Clear();

            hints?.ToList().ForEach(h => h?.Dispose());

            countUsedHints = 0;

            int countColumns;
            TypeHint[] types;

            switch (mode)
            {
                default:
                    types = new TypeHint[] { TypeHint.FiftyFifty, TypeHint.PhoneFriend, TypeHint.AskAudience };
                    countColumns = 3;
                    break;

                case Mode.Amateur:
                    types = new TypeHint[] { TypeHint.FiftyFifty, TypeHint.PhoneFriend, TypeHint.AskAudience, TypeHint.DoubleDip };
                    countColumns = 4;
                    break;

                case Mode.Advanced:
                    types = new TypeHint[] { TypeHint.AskHost, TypeHint.FiftyFifty, TypeHint.DoubleDip, TypeHint.PhoneFriend, TypeHint.SwitchQuestion };
                    countColumns = 3;
                    break;
            }

            int width = (int)(0.9f * Width / countColumns);
            Size sizeHint = new Size(width, (int)(0.63f * width));

            hints = types.Select(t => new ButtonHint(t)).ToArray();

            foreach (var hint in hints)
                hint.Click += OnHintClick;

            hiddenHints = new Queue<ButtonHint>(hints);

            SetLocationsHints(new Queue<ButtonHint>(hints), sizeHint, countColumns);
        }

        private void OnHintClick(object sender, EventArgs e)
        {
            ButtonHint hint = sender as ButtonHint;

            countUsedHints++;

            hint.Enabled = false;
            hint.Click -= OnHintClick;

            if (countUsedHints >= Hint.MaxCountAllowedHints)
                foreach (var h in hints.Where(x => x.Enabled))
                {
                    h.Click -= OnHintClick;
                    h.Lock();
                }

            HintClick.Invoke(hint.Type);
        }

        private void SetLocationsHints(Queue<ButtonHint> hints, Size sizeHint, int countColumns)
        {
            int countRows = (int)Math.Ceiling((float)hints.Count / countColumns);

            int x0 = (Size.Width - sizeHint.Width * countColumns) / 2;
            int y0 = (Size.Height - sizeHint.Height * countRows) / 2;

            int y;

            Queue<ButtonHint> queue = new Queue<ButtonHint>();
            ButtonHint hint;

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

        public void ShowHint()
        {
            hiddenHints.Dequeue().Show();

            int num = hints.Length - hiddenHints.Count;

            Sound.Play(num < 4 ? $"Rules_Hint{num}.wav" : $"CentralIcon_Show.wav");
        }

        public void ShowAllHints()
        {
            while (hiddenHints?.Count > 0)
                hiddenHints.Dequeue().Show();
        }
    }
}
