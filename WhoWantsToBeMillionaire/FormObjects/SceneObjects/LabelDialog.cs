using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelDialog : Label
    {
        private readonly StringFormat _format;
        private readonly Painter _painter;

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
                    _textRectangle = _painter.ResizeRectangle(_frameRectangle, _border);
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
            _painter = new Painter();
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

        public void SetRatioTextArea(float ratioWidth, float ratioHeight)
        {
            _frameRectangle = _painter.ResizeRectangle(ClientRectangle, ratioWidth, ratioHeight);
            _textRectangle = _painter.ResizeRectangle(_frameRectangle, _border);

            DrawBack();
        }

        private void DrawBack()
        {
            _backgroundText?.Dispose();
            _backgroundText = _painter.GetFilledPanel(_frameRectangle.Size, _border, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);

            Invalidate();
        }
    }
}