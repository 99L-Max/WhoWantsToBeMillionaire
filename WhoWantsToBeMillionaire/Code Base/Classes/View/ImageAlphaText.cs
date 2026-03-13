using System;
using System.Drawing;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    class ImageAlphaText : IDisposable
    {
        protected Rectangle TextRectangle;

        private int _lineLength = int.MaxValue;
        private int _alpha;
        private string _text;
        private Color _foreColor;
        private Font _font;

        public ImageAlphaText(Rectangle positionRectangle)
        {
            PositionRectangle = positionRectangle;
            ImageText = new Bitmap(PositionRectangle.Width, PositionRectangle.Height);

            _foreColor = Color.White;
            TextRectangle = new Rectangle(0, 0, PositionRectangle.Width, PositionRectangle.Height);
            FormatText = new StringFormat();
            _font = FontFactory.CreateFont(GameFonts.Arial, 0.25f * PositionRectangle.Height);
            G = Graphics.FromImage(ImageText);

            FormatText.Alignment = StringAlignment.Center;
            FormatText.LineAlignment = StringAlignment.Center;
        }

        public ImageAlphaText(int width, int height) : this(new Rectangle(0, 0, width, height)) { }

        public Rectangle PositionRectangle { get; }

        public Image ImageText { get; private set; }

        public string Text
        {
            get => _text;
            set => SetText(value);
        }

        public Font Font
        {
            set => SetFont(value);
            protected get => _font;
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

        public int LineLength
        {
            get => _lineLength;
            set => SetLineLength(value);
        }

        protected Graphics G { get; }

        protected StringFormat FormatText { get; }

        public virtual void Dispose()
        {
            G.Dispose();
            FormatText.Dispose();
            _font.Dispose();

            ImageText.Dispose();
        }

        public virtual void Reset()
        {
            _alpha = 0;
            G.Clear(Color.Transparent);
        }

        protected virtual void DrawText()
        {
            G.Clear(Color.Transparent);

            if (_alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(_alpha, _foreColor)))
                    G.DrawString(_text, _font, brush, TextRectangle, FormatText);
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

        private void SetLineLength(int lineLength)
        {
            if (_lineLength != lineLength)
            {
                _lineLength = lineLength;
                DrawText();
            }
        }

        private void SetText(string text)
        {
            if (text.Length > _lineLength)
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
    }
}