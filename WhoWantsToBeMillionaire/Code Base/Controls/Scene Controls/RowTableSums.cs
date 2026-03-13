using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class RowTableSums : PictureBox, IResettable
    {
        private static readonly Image s_background;
        private static readonly Image s_iconCircle;
        private static readonly Image s_iconRhomb;
        private static readonly Size s_sizeNumber;
        private static readonly Size s_sizeSum;
        private static readonly TextFormatFlags s_textFormatFlags;

        private readonly Image _image;

        private bool _isSelected;
        private bool _iconVisible;
        private bool _isSavingSum;
        private bool _isMouseEventsActive = false;

        static RowTableSums()
        {
            var height = 60;

            s_background = new Bitmap(Resources.CurrentAmount, 500, height);
            s_iconRhomb = new Bitmap(Resources.IconSum_Rhomb, height, height);
            s_iconCircle = Painter.GetGradientEllipse(new Size(height, height), Color.White);

            s_sizeNumber = new Size((int)(0.18f * s_background.Width), s_background.Height);
            s_sizeSum = new Size((int)(0.90f * s_background.Width), s_background.Height);
            s_textFormatFlags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
        }

        public RowTableSums(int number, int sum)
        {
            Number = number;
            Sum = sum;
            Font = FontFactory.CreateFont(GameFonts.Arial, 0.5f * s_background.Height, FontStyle.Bold);
            Dock = DockStyle.Fill;

            _image = new Bitmap(s_background.Width, s_background.Height);
        }

        public int Number { get; }

        public int Sum { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetBoolFlag(ref _isSelected, value);
        }

        public bool IsIconVisible
        {
            get => _iconVisible;
            set => SetBoolFlag(ref _iconVisible, value);
        }

        public bool IsSavingSum
        {
            get => _isSavingSum;
            set => SetBoolFlag(ref _isSavingSum, value);
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            _iconVisible = _isSavingSum = _isSelected = false;

            RemoveMouseEvents();
            DrawImage();
            Invalidate();
        }

        public void AddMouseEvents()
        {
            if (_isMouseEventsActive == false)
            {
                _isMouseEventsActive = true;

                MouseEnter += OnRowMouseEnter;
                MouseLeave += OnRowMouseLeave;
            }
        }

        public void RemoveMouseEvents()
        {
            if (_isMouseEventsActive)
            {
                _isMouseEventsActive = false;

                MouseEnter -= OnRowMouseEnter;
                MouseLeave -= OnRowMouseLeave;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_isSelected)
                e.Graphics.DrawImage(s_background, ClientRectangle);

            e.Graphics.DrawImage(_image, ClientRectangle);
        }

        private void DrawImage()
        {
            using (Graphics g = Graphics.FromImage(_image))
            {
                g.Clear(Color.Transparent);

                Color[] colors = { Color.Black, _isSavingSum ? Color.White : Color.Orange };
                Point[] points = { new Point(2, 2), new Point() };

                for (int i = 0; i < points.Length; i++)
                {
                    TextRenderer.DrawText(g, $"{Number}", Font, new Rectangle(points[i], s_sizeNumber), colors[i], s_textFormatFlags);
                    TextRenderer.DrawText(g, String.Format("{0:#,0}", Sum), Font, new Rectangle(points[i], s_sizeSum), colors[i], s_textFormatFlags);
                }

                if (_iconVisible)
                    g.DrawImage(_isSelected ? s_iconCircle : s_iconRhomb, s_sizeNumber.Width, 0, s_background.Height, s_background.Height);
            }
        }

        private void OnRowMouseLeave(object sender, EventArgs e)
        {
            IsSelected = false;
        }

        private void OnRowMouseEnter(object sender, EventArgs e)
        {
            IsSelected = true;
        }

        private void SetBoolFlag(ref bool field, bool value)
        {
            if (field != value)
            {
                field = value;

                DrawImage();
                Invalidate();
            }
        }
    }
}