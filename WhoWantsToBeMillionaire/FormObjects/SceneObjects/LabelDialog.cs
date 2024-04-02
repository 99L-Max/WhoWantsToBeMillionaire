using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelDialog : Label
    {
        private readonly StringFormat _format;

        private int _border = 5;
        private Rectangle _textRectangle;
        private Rectangle _frameRectangle;
        private Image _backgroundText;

        public int Border
        {
            set
            {
                if (_border != value)
                {
                    _border = value;
                    SetBorder(value);
                    DrawBack();
                }
            }
            get => _border;
        }

        public LabelDialog(float sizeFont)
        {
            Dock = DockStyle.Fill;
            Font = new Font("", sizeFont, GraphicsUnit.Pixel);

            _textRectangle = _frameRectangle = new Rectangle();
            _format = new StringFormat();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Text != string.Empty)
            {
                e.Graphics.DrawImage(_backgroundText, _frameRectangle);
                e.Graphics.DrawString(Text, Font, Brushes.White, _textRectangle, _format);
            }
            else
            {
                base.OnPaint(e);
            }
        }

        public void SetAlignment(StringAlignment vertical, StringAlignment horizontal)
        {
            _format.LineAlignment = vertical;
            _format.Alignment = horizontal;
        }

        public void SetRatioText(float ratioWidth, float ratioHeight)
        {
            _frameRectangle.Width = (int)(ClientRectangle.Width * ratioWidth);
            _frameRectangle.Height = (int)(ClientRectangle.Height * ratioHeight);

            _frameRectangle.X = (ClientRectangle.Width - _frameRectangle.Width) >> 1;
            _frameRectangle.Y = (ClientRectangle.Height - _frameRectangle.Height) >> 1;

            SetBorder(_border);
            DrawBack();
        }

        private void SetBorder(int border)
        {
            _textRectangle.Width = _frameRectangle.Width - (border << 1);
            _textRectangle.Height = _frameRectangle.Height - (border << 1);

            _textRectangle.X = _frameRectangle.X + border;
            _textRectangle.Y = _frameRectangle.Y + border;
        }

        private void DrawBack()
        {
            _backgroundText?.Dispose();
            _backgroundText = new Bitmap(_frameRectangle.Width, _frameRectangle.Height);

            var frame = new Rectangle(0, 0, _frameRectangle.Width, _frameRectangle.Height);
            var fill = new Rectangle(_border, _border, _textRectangle.Width, _textRectangle.Height);

            using (Graphics g = Graphics.FromImage(_backgroundText))
            using (LinearGradientBrush brushFrame = new LinearGradientBrush(frame, Color.Gainsboro, Color.SlateGray, 45f))
            using (LinearGradientBrush brushFill = new LinearGradientBrush(fill, Color.Navy, Color.Black, 90f))
            {
                g.FillRectangle(brushFrame, frame);
                g.FillRectangle(brushFill, fill);
            }

            Invalidate();
        }
    }
}
