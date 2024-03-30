using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButtonWire
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    class ButtonWire : PictureBox, IDisposable
    {
        private static readonly ReadOnlyDictionary<ThemeButtonWire, Image> _imageButton;
        private static readonly Image _wire;

        private readonly Label _leftBarrier;
        private readonly Label _rightBarrier;

        private Rectangle _imageRectangle;
        private Rectangle _backgroundRectangle;
        private Color _foreColor;
        private ThemeButtonWire _theme;

        static ButtonWire()
        {
            var keys = Enum.GetValues(typeof(ThemeButtonWire)).Cast<ThemeButtonWire>();
            var img = keys.ToDictionary(k => k, v => ResourceManager.GetImage($"ButtonWire_{v}.png"));

            _imageButton = new ReadOnlyDictionary<ThemeButtonWire, Image>(img);
            _wire = ResourceManager.GetImage("Wire.png");
        }

        public ButtonWire(float sizeFont)
        {
            BackColor = Color.Transparent;
            Dock = DockStyle.Fill;
            Font = new Font("", sizeFont, FontStyle.Bold, GraphicsUnit.Pixel);

            _foreColor = Color.White;
            _theme = ThemeButtonWire.Blue;

            _leftBarrier = new Label();
            _rightBarrier = new Label();

            Controls.Add(_leftBarrier);
            Controls.Add(_rightBarrier);

            SizeChanged += OnSizeChanged;
            EnabledChanged += OnEnabledChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_wire, _backgroundRectangle);
            e.Graphics.DrawImage(_imageButton[_theme], _imageRectangle);

            TextRenderer.DrawText(e.Graphics, Text, Font, _imageRectangle, _foreColor);
        }

        private void SetImageAndForeColor(ThemeButtonWire theme, Color foreColor)
        {
            _theme = theme;
            _foreColor = foreColor;

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) =>
            SetImageAndForeColor(ThemeButtonWire.Orange, Color.Black);

        protected override void OnMouseLeave(EventArgs e) =>
            SetImageAndForeColor(ThemeButtonWire.Blue, Color.White);

        protected override void OnMouseDown(MouseEventArgs e) =>
            SetImageAndForeColor(ThemeButtonWire.Green, Color.Black);

        protected override void OnMouseUp(MouseEventArgs e) =>
            SetImageAndForeColor(ThemeButtonWire.Orange, Color.Black);

        private void OnSizeChanged(object sender, EventArgs e)
        {
            Size sizeImage = _imageButton[ThemeButtonWire.Blue].Size;

            float wfactor = (float)sizeImage.Width / Width;
            float hfactor = (float)sizeImage.Height / Height;

            float resizeFactor = Math.Max(wfactor, hfactor);

            Size sizeRect = new Size((int)(sizeImage.Width / resizeFactor), (int)(sizeImage.Height / resizeFactor));

            int x = (Width - sizeRect.Width) >> 1;
            int y = (Height - sizeRect.Height) >> 1;

            _imageRectangle = new Rectangle(x, y, sizeRect.Width, sizeRect.Height);
            _backgroundRectangle = new Rectangle(0, y, Width, sizeRect.Height);

            _rightBarrier.Size = _leftBarrier.Size = new Size((Width - _imageRectangle.Width) >> 1, Height);
            _rightBarrier.Location = new Point(Width - _rightBarrier.Width, 0);
        }

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            if(Enabled)
                SetImageAndForeColor(ThemeButtonWire.Blue, Color.White);
            else
                SetImageAndForeColor(ThemeButtonWire.Gray, Color.Black);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SizeChanged -= OnSizeChanged;
                EnabledChanged -= OnEnabledChanged;

                _leftBarrier.Dispose();
                _rightBarrier.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}