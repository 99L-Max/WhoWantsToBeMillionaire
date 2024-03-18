using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    class TableHints : GameContol, IReset, IGameSettings
    {
        private List<ButtonHint> hints;
        private Queue<ButtonHint> hiddenHints;
        private int countUsedHints;
        private bool toolTipVisible;

        public int CountHints => hints.Count;

        public string DescriptionNextHint => hiddenHints.Peek().Description;

        public bool AllHintsVisible => hiddenHints.Count == 0;

        public string TextActiveHints
        {
            get
            {
                int count = hints.Select(h => h.Enabled).Count();

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

        public void Reset(Mode mode = Mode.Classic)
        {
            Visible = Enabled = false;

            Controls.Clear();

            hints?.ForEach(h => { h.Click -= OnHintClick; h.Dispose(); });

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
            var dict = ResourceManager.GetDictionary("DescriptionHints.json");

            hints = types.Select(t => new ButtonHint(t, dict[t.ToString()], toolTipVisible)).ToList();
            hints.ForEach(h => h.Click += OnHintClick);

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

            int x0 = (Size.Width - sizeHint.Width * countColumns) >> 1;
            int y0 = (Size.Height - sizeHint.Height * countRows) >> 1;

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
                    x0 = (Size.Width - sizeHint.Width * queue.Count) >> 1;
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
            hiddenHints.Dequeue().ShowIcon();

            int num = hints.Count - hiddenHints.Count;

            Sound.Play(num < 4 ? $"Rules_Hint{num}.wav" : $"CentralIcon_Show.wav");
        }

        public void ShowAllHints()
        {
            while (hiddenHints.Count > 0)
                hiddenHints.Dequeue().ShowIcon();
        }

        public void SetSettings(GameSettingsData data)
        {
            toolTipVisible = Convert.ToBoolean(data.GetSettings(GameSettings.ShowDescriptionHints));
            hints?.ForEach(h => h.ToolTipVisible = toolTipVisible);
        }
    }
}
