using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameComboBox : PictureBox, IDisposable, IResizable
    {
        private readonly List<float> _values;
        private readonly List<string> _texts;
        private readonly ButtonArrow _leftArrow;
        private readonly ButtonArrow _rightArrow;

        private int _selectedIndex = -1;

        public GameComboBox(Dictionary<float, string> items, float fontSize)
        {
            Font = FontFactory.CreateFont(GameFonts.Arial, fontSize);
            ForeColor = Color.White;

            _values = items.Keys.ToList();
            _texts = items.Values.ToList();

            _leftArrow = new ButtonArrow(ButtonArrowDirections.Left);
            _rightArrow = new ButtonArrow(ButtonArrowDirections.Right);

            _leftArrow.Click += OnLeftClick;
            _rightArrow.Click += OnRightClick;

            _leftArrow.DoubleClick += OnLeftClick;
            _rightArrow.DoubleClick += OnRightClick;

            Controls.Add(_leftArrow);
            Controls.Add(_rightArrow);
        }

        public event Action<object, EventArgs> SelectedIndexChanged;

        public bool Looped { get; set; } = true;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetSelectedIndex(value);
        }

        public float SelectedValue
        {
            get => _selectedIndex == -1 ? float.NaN : _values[_selectedIndex];
            set => SelectedIndex = _values.IndexOf(value);
        }

        public void AlignSize()
        {
            _leftArrow.Size = _rightArrow.Size = Resizer.Resize(ClientSize, 0.15f, 1f);

            Font?.Dispose();
            Font = FontFactory.CreateFont(GameFonts.Arial, 0.45f * ClientRectangle.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, _texts[_selectedIndex], Font, ClientRectangle, ForeColor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _leftArrow.Click -= OnLeftClick;
                _rightArrow.Click -= OnRightClick;

                _leftArrow.DoubleClick -= OnLeftClick;
                _rightArrow.DoubleClick -= OnRightClick;

                _leftArrow.Dispose();
                _rightArrow.Dispose();

                BackgroundImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnLeftClick(object sender, EventArgs e)
        {
            if (Looped)
                SelectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : _values.Count - 1;
            else
                SelectedIndex = Math.Max(0, _selectedIndex - 1);
        }

        private void OnRightClick(object sender, EventArgs e)
        {
            if (Looped)
                SelectedIndex = _selectedIndex < _values.Count - 1 ? _selectedIndex + 1 : 0;
            else
                SelectedIndex = Math.Min(_selectedIndex + 1, _values.Count - 1);
        }

        private void SetSelectedIndex(int value)
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;

                Invalidate();

                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
