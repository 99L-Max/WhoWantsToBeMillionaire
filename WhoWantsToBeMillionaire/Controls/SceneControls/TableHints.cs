using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableHints : GameContol, IReset, ISetSettings
    {
        private List<ButtonHint> _hints;
        private bool _toolTipVisible;
        private int _countShownHints;

        public Action<TypeHint> HintClick;

        public int CountUsedHints { get; private set; }

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
            _hints.ForEach(h => { h.Click += OnHintClick; h.ToolTipVisible = _toolTipVisible; });

            SetBoundsHints(new List<ButtonHint>(_hints), sizeHint, countColumns);
        }

        private void SetBoundsHints(IEnumerable<ButtonHint> hints, Size hintSize, int columnCount)
        {
            var rowCount = (int)Math.Ceiling((float)hints.Count() / columnCount);
            var rowRectangle = new Rectangle();

            rowRectangle.Width = hintSize.Width * columnCount;
            rowRectangle.Height = hintSize.Height;

            rowRectangle.X = Size.Width - rowRectangle.Width >> 1;
            rowRectangle.Y = Size.Height - hintSize.Height * rowCount >> 1;

            int countInRow, rowX0;
            IEnumerable<ButtonHint> listRow;
            ButtonHint button;

            for (int row = 0; row < rowCount; row++)
            {
                countInRow = Math.Min(columnCount, hints.Count());

                listRow = hints.Take(countInRow);
                hints = hints.Skip(countInRow);

                for (int i = 0; i < listRow.Count(); i++)
                {
                    button = listRow.ElementAt(i);
                    rowX0 = (rowRectangle.Width - hintSize.Width * listRow.Count() >> 1) + rowRectangle.X;

                    button.Location = new Point(rowX0 + i * hintSize.Width, rowRectangle.Y);
                    button.Size = hintSize;

                    Controls.Add(button);
                }

                rowRectangle.Y += rowRectangle.Height;
            }
        }

        public void SetSettings(GameSettingsData data)
        {
            _toolTipVisible = Convert.ToBoolean(data.GetSettings(GameSettings.ShowDescriptionHints));
            _hints?.ForEach(h => h.ToolTipVisible = _toolTipVisible);
        }

        private void OnHintClick(object sender, EventArgs e)
        {
            if (sender is ButtonHint hint)
            {
                hint.Enabled = false;
                hint.Click -= OnHintClick;

                if (++CountUsedHints >= Hint.MaxCountAllowedHints)
                    foreach (var h in _hints)
                        if (h.Enabled)
                        {
                            h.Click -= OnHintClick;
                            h.Lock();
                        }

                HintClick?.Invoke(hint.Type);
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
    }
}
