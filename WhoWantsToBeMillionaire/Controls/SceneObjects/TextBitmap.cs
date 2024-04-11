using System.Drawing;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    class TextBitmap
    {
        private int _lengthLine = int.MaxValue;

        protected readonly Graphics _g;
        protected readonly StringFormat _formatText;

        protected int _alpha;
        protected string _text;
        protected Font _font;

        public readonly Rectangle Rectangle;

        public float SizeFont
        {
            set
            {
                _font.Dispose();
                _font = new Font("", value, GraphicsUnit.Pixel);

                DrawText();
            }
        }

        public string Text
        {
            get => _text;
            set { _text = FormatText(value); DrawText(); }
        }

        public int Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    DrawText();
                }
            }
        }

        public int LengthLine
        {
            get => _lengthLine;
            set { _lengthLine = value; DrawText(); }
        }

        public Image ImageText { private set; get; }

        public TextBitmap(int width, int height) : this(new Rectangle(0, 0, width, height)) { }

        public TextBitmap(Rectangle rectangle)
        {
            Rectangle = rectangle;
            ImageText = new Bitmap(Rectangle.Width, Rectangle.Height);

            _formatText = new StringFormat();
            _font = new Font("", 0.25f * Rectangle.Height, GraphicsUnit.Pixel);
            _g = Graphics.FromImage(ImageText);

            _formatText.Alignment = StringAlignment.Center;
            _formatText.LineAlignment = StringAlignment.Center;
        }

        private string FormatText(string text)
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

                return builder.ToString();
            }

            return text;
        }

        protected virtual void DrawText()
        {
            _g.Clear(Color.Transparent);

            if (_alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(_alpha, Color.White)))
                    _g.DrawString(_text, _font, brush, Rectangle, _formatText);
        }

        public virtual void Reset()
        {
            _alpha = 0;
            _g.Clear(Color.Transparent);
        }
    }
}
