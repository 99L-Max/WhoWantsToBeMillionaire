using System.Drawing;
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

        public LabelDialog(float sizeFont)
        {
            Dock = DockStyle.Fill;
            Font = FontManager.CreateFont(GameFont.Arial, sizeFont);

            _textRectangle = _frameRectangle = new Rectangle();
            _format = new StringFormat();
        }

        public int Border
        {
            get => _border;
            set => SetBorder(value);
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

        private void SetBorder(int border)
        {
            if (_border != border)
            {
                _border = border;
                _textRectangle = Resizer.ResizeRectangle(_frameRectangle, _border);

                DrawBack();
            }
        }

        public void SetAlignment(StringAlignment vertical, StringAlignment horizontal)
        {
            _format.LineAlignment = vertical;
            _format.Alignment = horizontal;
        }

        public void SetRatioTextArea(float ratioWidth, float ratioHeight)
        {
            _frameRectangle = Resizer.ResizeRectangle(ClientRectangle, ratioWidth, ratioHeight);
            _textRectangle = Resizer.ResizeRectangle(_frameRectangle, _border);

            DrawBack();
        }

        private void DrawBack()
        {
            _backgroundText?.Dispose();
            _backgroundText = Painter.CreateFilledPanel(_frameRectangle.Size, _border, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);

            Invalidate();
        }
    }
}