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
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = ResourceProcessing.GetImage("ButtonModeSelection.png");
            Font = new Font("", fontSize);
            ForeColor = Color.White;
            Dock = DockStyle.Fill;
            TextAlign = ContentAlignment.MiddleCenter;

            this.items = items;

            leftArrow = new ButtonArrow(DirectionArrow.Left, 2 * fontSize);
            rightArrow = new ButtonArrow(DirectionArrow.Right, 2 * fontSize);

            Controls.Add(leftArrow);
            Controls.Add(rightArrow);

            leftArrow.Click += OnArrowClick;
            rightArrow.Click += OnArrowClick;

            SizeChanged += OnSizeChanged;
        }

        private void OnArrowClick(object sender, EventArgs e)
        {
            if ((sender as ButtonArrow).DirectionArrow == DirectionArrow.Right)
            {
                SelectedIndex = (SelectedIndex + 1) % items.Length;
            }
            else
            {
                SelectedIndex = (SelectedIndex == 0 ? items.Length : SelectedIndex) - 1;
            }
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
                leftArrow.Click -= OnArrowClick;
                rightArrow.Click -= OnArrowClick;

                leftArrow.Dispose();
                rightArrow.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
