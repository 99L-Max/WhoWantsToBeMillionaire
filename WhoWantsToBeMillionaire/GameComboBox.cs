using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameComboBox : Label, IDisposable
    {
        private int selectedIndex = -1;

        private readonly string[] items;
        private readonly ButtonArrow leftArrow;
        private readonly ButtonArrow rightArrow;

        public delegate void EventSelectedIndexChanged();
        public EventSelectedIndexChanged SelectedIndexChanged;

        public bool LoopedSwitch = true;

        public int SelectedIndex
        {
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;

                    Text = items[selectedIndex];

                    SelectedIndexChanged.Invoke();
                }
            }
            get => selectedIndex;
        }

        public GameComboBox(string[] items, float fontSize)
        {
            Font = new Font("", fontSize);
            ForeColor = Color.White;
            Dock = DockStyle.Fill;
            TextAlign = ContentAlignment.MiddleCenter;

            this.items = items;

            leftArrow = new ButtonArrow(DirectionArrow.Left);
            rightArrow = new ButtonArrow(DirectionArrow.Right);

            Controls.Add(leftArrow);
            Controls.Add(rightArrow);

            leftArrow.Click += OnLeftClick;
            rightArrow.Click += OnRightClick;

            leftArrow.DoubleClick += OnLeftClick;
            rightArrow.DoubleClick += OnRightClick;

            SizeChanged += OnSizeChanged;
        }

        private void OnLeftClick(object sender, EventArgs e)
        {
            if (LoopedSwitch)
                SelectedIndex = selectedIndex > 1 ? selectedIndex - 1 : items.Length - 1;
            else
                SelectedIndex = Math.Max(0, selectedIndex - 1);
        }

        private void OnRightClick(object sender, EventArgs e)
        {
            if (LoopedSwitch)
                SelectedIndex = selectedIndex < items.Length - 1 ? selectedIndex + 1 : 0;
            else
                SelectedIndex = Math.Min(selectedIndex + 1, items.Length - 1);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            int width = (int)(0.15f * ClientRectangle.Width);
            int height = ClientRectangle.Height;

            leftArrow.Size = rightArrow.Size = new Size(width, height);

            rightArrow.Location = new Point(ClientRectangle.Width - width, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                leftArrow.Click -= OnLeftClick;
                rightArrow.Click -= OnRightClick;

                leftArrow.DoubleClick -= OnLeftClick;
                rightArrow.DoubleClick -= OnRightClick;

                SizeChanged -= OnSizeChanged;

                leftArrow.Dispose();
                rightArrow.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
