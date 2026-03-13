using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableHints : GameContol, IResettable, ISettable
    {
        private List<HintButton> _hintButtons = new List<HintButton>();
        private bool _toolTipVisible;
        private int _countShownHints;

        public TableHints(int width, int height) : base(width, height) { }

        public event Action<HintTypes> HintClick;

        public int UsedHintsCount { get; private set; }

        public int HintsCount => _hintButtons.Count;

        public string DescriptionNextHint => _hintButtons[_countShownHints].Description;

        public bool AreAllHintsShown => _hintButtons.All(hint => hint.IsShown);

        public string TextActiveHints
        {
            get
            {
                var count = _hintButtons.Count(hint => hint.Status == HintStatuses.Active);
                var lastNumber = count % 10;

                if (lastNumber == 1)
                    return $"{count} подсказка";
                else if (lastNumber > 1 && lastNumber < 5)
                    return $"{count} подсказки";
                else
                    return $"{count} подсказок";
            }
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            Enabled = false;
            UsedHintsCount = _countShownHints = 0;
            Controls.Clear();

            var countColumns = mode == Modes.Medium ? 4 : 3;
            var widthHint = (int)(0.9f * Width / countColumns);
            var hintSize = Resizer.Resize(BasicSizes.Width, widthHint, 100, 63);
            var types = JsonReader.GetDictionary<Modes, ModeData>(Resources.Dictionary_ModeData)[mode].Hints;

            SetButtons(types);
            SetBoundsHints(_hintButtons, hintSize, countColumns);
        }

        public void SetSettings(SettingsData data)
        {
            _toolTipVisible = Convert.ToBoolean(data.GetSettings(GameSettings.ShowDescriptionHints));
            _hintButtons?.ForEach(button => button.ToolTipVisible = _toolTipVisible);
        }

        public void ShowHint()
        {
            _hintButtons[_countShownHints++].ShowIcon();

            if (_countShownHints < 4)
                Sound.Play($"Rules_Hint{_countShownHints}");
            else
                Sound.Play(Resources.CentralIcon_Show);
        }

        public void ShowAllHints()
        {
            _hintButtons.ForEach(button => button.ShowIcon());
        }

        private void OnHintButtonClick(object sender, EventArgs e)
        {
            if (sender is HintButton button)
            {
                button.Enabled = false;
                button.Click -= OnHintButtonClick;

                if (++UsedHintsCount >= Hint.MaxAllowedHintsCount)
                {
                    foreach (var hint in _hintButtons)
                    {
                        if (hint.Enabled)
                        {
                            hint.Click -= OnHintButtonClick;
                            hint.Lock();
                        }
                    }
                }

                HintClick?.Invoke(button.Type);
            }
        }

        private void SetBoundsHints(IEnumerable<HintButton> buttons, Size buttonSize, int columnCount)
        {
            var rowCount = (int)Math.Ceiling((float)buttons.Count() / columnCount);
            var rowRectangle = new Rectangle();

            rowRectangle.Width = buttonSize.Width * columnCount;
            rowRectangle.Height = buttonSize.Height;

            rowRectangle.X = Size.Width - rowRectangle.Width >> 1;
            rowRectangle.Y = Size.Height - buttonSize.Height * rowCount >> 1;

            int countInRow, rowX0;
            IEnumerable<HintButton> rowsButtons;
            HintButton button;

            for (int row = 0; row < rowCount; row++)
            {
                countInRow = Math.Min(columnCount, buttons.Count());

                rowsButtons = buttons.Take(countInRow);
                buttons = buttons.Skip(countInRow);

                for (int i = 0; i < rowsButtons.Count(); i++)
                {
                    button = rowsButtons.ElementAt(i);
                    rowX0 = (rowRectangle.Width - buttonSize.Width * rowsButtons.Count() >> 1) + rowRectangle.X;

                    button.Location = new Point(rowX0 + i * buttonSize.Width, rowRectangle.Y);
                    button.Size = buttonSize;

                    Controls.Add(button);
                }

                rowRectangle.Y += rowRectangle.Height;
            }
        }

        private void SetButtons(HintTypes[] types)
        {
            var descriptions = JsonReader.GetDictionary<HintTypes, string>(Resources.Dictionary_DescriptionHints);

            foreach (var button in _hintButtons)
            {
                button.Click -= OnHintButtonClick;
                button.Dispose();
            }

            _hintButtons.Clear();

            foreach (var type in types)
            {
                var button = new HintButton(type, descriptions[type]) { ToolTipVisible = _toolTipVisible };
                button.Click += OnHintButtonClick;
                _hintButtons.Add(button);
            }
        }
    }
}
