using System;
using System.Drawing;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    class ImageAlphaText : IDisposable
    {
        private int _lengthLine = int.MaxValue;

        protected readonly Graphics _g;
        protected readonly StringFormat _formatText;

        protected int _alpha;
        protected string _text;
        protected Color _foreColor;
        protected Rectangle _textRectangle;
        protected Font _font;

        public readonly Rectangle PositionRectangle;

        public ImageAlphaText(Rectangle positionRectangle)
        {
            PositionRectangle = positionRectangle;
            ImageText = new Bitmap(PositionRectangle.Width, PositionRectangle.Height);

            _foreColor = Color.White;
            _textRectangle = new Rectangle(0, 0, PositionRectangle.Width, PositionRectangle.Height);
            _formatText = new StringFormat();
            _font = new Font("", 0.25f * PositionRectangle.Height, GraphicsUnit.Pixel);
            _g = Graphics.FromImage(ImageText);

            _formatText.Alignment = StringAlignment.Center;
            _formatText.LineAlignment = StringAlignment.Center;
        }

        public ImageAlphaText(int width, int height) : this(new Rectangle(0, 0, width, height)) { }

        public Image ImageText { get; private set; }

        public string Text
        {
            get => _text;
            set => SetText(value);
        }

        public Font Font
        {
            set => SetFont(value);
        }

        public Color ForeColor
        {
            get => _foreColor;
            set => SetForeColor(value);
        }

        public int Alpha
        {
            get => _alpha;
            set => SetAlpha(value);
        }

        public int LengthLine
        {
            get => _lengthLine;
            set => SetLengthLine(value);
        }

        public virtual void Dispose()
        {
            _g.Dispose();
            _formatText.Dispose();
            _font.Dispose();

            ImageText.Dispose();
        }

        private void SetAlpha(int alpha)
        {
            if (_alpha != alpha)
            {
                _alpha = alpha;
                DrawText();
            }
        }

        private void SetFont(Font font)
        {
            _font?.Dispose();
            _font = font;

            DrawText();
        }

        private void SetForeColor(Color foreColor)
        {
            if (_foreColor != foreColor)
            {
                _foreColor = foreColor;
                DrawText();
            }
        }

        private void SetLengthLine(int lengthLine)
        {
            if (_lengthLine != lengthLine)
            {
                _lengthLine = lengthLine;
                DrawText();
            }
        }

        private void SetText(string text)
        {
            if (text.Length > _lengthLine)
            {
                var builder = new StringBuilder(text);
                var middle = text.Length >> 1;
                var index = middle;

                for (int i = 0; i < middle; i++)
                {
                    index += (i & 1) == 0 ? i : -i;

                    if (builder[index] == ' ')
                    {
                        builder[index] = '\n';
                        break;
                    }
                }

                _text = builder.ToString();
            }
            else
            {
                _text = text;
            }

            DrawText();
        }

        protected virtual void DrawText()
        {
            _g.Clear(Color.Transparent);

            if (_alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(_alpha, _foreColor)))
                    _g.DrawString(_text, _font, brush, _textRectangle, _formatText);
        }

        public virtual void Reset()
        {
            _alpha = 0;
            _g.Clear(Color.Transparent);
        }
    }
}