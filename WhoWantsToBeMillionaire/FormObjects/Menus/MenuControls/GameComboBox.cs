using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameComboBox : Label, IDisposable
    {
        private readonly string[] _items;
        private readonly ButtonArrow _leftArrow;
        private readonly ButtonArrow _rightArrow;

        private int _selectedIndex = -1;

        public delegate void EventSelectedIndexChanged(object sender, EventArgs e);
        public EventSelectedIndexChanged SelectedIndexChanged;

        public bool LoopedSwitch = true;

        public int SelectedIndex
        {
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;

                    Text = _items[_selectedIndex];

                    SelectedIndexChanged.Invoke(this, EventArgs.Empty);
                }
            }
            get => _selectedIndex;
        }

        public GameComboBox(string[] items, float fontSize)
        {
            Font = new Font("", fontSize, GraphicsUnit.Pixel);
            ForeColor = Color.White;
            Dock = DockStyle.Fill;
            TextAlign = ContentAlignment.MiddleCenter;

            _items = items;

            _leftArrow = new ButtonArrow(DirectionArrow.Left);
            _rightArrow = new ButtonArrow(DirectionArrow.Right);

            _leftArrow.Click += OnLeftClick;
            _rightArrow.Click += OnRightClick;

            _leftArrow.DoubleClick += OnLeftClick;
            _rightArrow.DoubleClick += OnRightClick;

            Controls.Add(_leftArrow);
            Controls.Add(_rightArrow);
        }

        private void OnLeftClick(object sender, EventArgs e)
        {
            if (LoopedSwitch)
                SelectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : _items.Length - 1;
            else
                SelectedIndex = Math.Max(0, _selectedIndex - 1);
        }

        private void OnRightClick(object sender, EventArgs e)
        {
            if (LoopedSwitch)
                SelectedIndex = _selectedIndex < _items.Length - 1 ? _selectedIndex + 1 : 0;
            else
                SelectedIndex = Math.Min(_selectedIndex + 1, _items.Length - 1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            int width = (int)(0.15f * ClientRectangle.Width);
            int height = ClientRectangle.Height;

            _leftArrow.Size = _rightArrow.Size = new Size(width, height);

            _rightArrow.Location = new Point(ClientRectangle.Width - width, 0);
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
    }
}
