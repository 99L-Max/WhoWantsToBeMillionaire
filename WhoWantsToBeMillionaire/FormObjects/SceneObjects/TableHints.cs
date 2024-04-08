using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableHints : GameContol, IReset, IGameSettings
    {
        private List<ButtonHint> _hints;
        private bool _toolTipVisible;
        private int _countShownHints;

        public int CountUsedHints { private set; get; }

        public int CountHints =>
            _hints.Count;

        public string DescriptionNextHint =>
            _hints[_countShownHints].Description;

        public bool AllHintsShown =>
            _hints.All(x => x.IsShown);

        public string TextActiveHints
        {
            get
            {
                int count = _hints.Select(h => h.Enabled).Count();

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
            Enabled = false;
            CountUsedHints = _countShownHints = 0;
            Controls.Clear();

            var countColumns = mode == Mode.Amateur ? 4 : 3;
            var types = JsonManager.GetDictionary<Mode, TypeHint[]>(Resources.Settings_Mode)[mode];
            var width = (int)(0.9f * Width / countColumns);
            var sizeHint = new Size(width, (int)(0.63f * width));
            var descriptions = JsonManager.GetDictionary<TypeHint, string>(Resources.Dictionary_DescriptionHints);

            _hints?.ForEach(h => { h.Click -= OnHintClick; h.Dispose(); });
            _hints = types.Select(t => new ButtonHint(t, descriptions[t])).ToList();

            SetLocationsHints(new Queue<ButtonHint>(_hints), sizeHint, countColumns);

            _hints.ForEach(h => { h.Click += OnHintClick; h.ToolTipVisible = _toolTipVisible; });
        }

        private void OnHintClick(object sender, EventArgs e)
        {
            ButtonHint hint = sender as ButtonHint;

            hint.Enabled = false;
            hint.Click -= OnHintClick;

            if (++CountUsedHints >= Hint.MaxCountAllowedHints)
                foreach (var h in _hints)
                    if (h.Enabled)
                    {
                        h.Click -= OnHintClick;
                        h.Lock();
                    }

            HintClick.Invoke(hint.Type);
        }

        private void SetLocationsHints(Queue<ButtonHint> hints, Size sizeHint, int countColumns)
        {
            int countRows = (int)Math.Ceiling((float)hints.Count / countColumns);

            int x0 = Size.Width - sizeHint.Width * countColumns >> 1;
            int y0 = Size.Height - sizeHint.Height * countRows >> 1;

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
                    x0 = Size.Width - sizeHint.Width * queue.Count >> 1;
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
            _hints[_countShownHints++].ShowIcon();

            if (_countShownHints < 4)
                Sound.Play($"Rules_Hint{_countShownHints}");
            else
                Sound.Play(Resources.CentralIcon_Show);
        }

        public void ShowAllHints() =>
            _hints.ForEach(h => h.ShowIcon());

        public void SetSettings(GameSettingsData data)
        {
            _toolTipVisible = Convert.ToBoolean(data.GetSettings(GameSettings.ShowDescriptionHints));
            _hints?.ForEach(h => h.ToolTipVisible = _toolTipVisible);
        }
    }
}
