using System.Drawing;
using System.Text;

namespace WhoWantsToBeMillionaire
{
    class TextBitmap
    {
        private readonly int lengthLine;

        protected readonly Graphics g;
        protected readonly StringFormat formatText;

        protected int alpha;
        protected string text;
        protected Font font;

        public readonly Rectangle Rectangle;

        public float SizeFont
        {
            set
            {
                font.Dispose();
                font = new Font("", value, GraphicsUnit.Pixel);
                DrawText();
            }
        }

        public string Text
        {
            set
            {
                if (value.Length > lengthLine)
                {
                    StringBuilder builder = new StringBuilder(value);
                    int middle = value.Length >> 1;
                    int index = middle;

                    for (int i = 0; i < middle; i++)
                    {
                        index += (i & 1) == 0 ? i : -i;

                        if (builder[index] == ' ')
                        {
                            builder[index] = '\n';
                            break;
                        }
                    }

                    text = builder.ToString();
                }
                else
                {
                    text = value;
                }

                DrawText();
            }

            get => text;
        }

        public int Alpha
        {
            set
            {
                if (alpha != value)
                {
                    alpha = value;
                    DrawText();
                }
            }
        }

        public Image ImageText { private set; get; }

        public TextBitmap(int width, int height) : this(new Rectangle(0, 0, width, height)) { }

        public TextBitmap(Rectangle rectangle, int lengthLine = int.MaxValue)
        {
            this.lengthLine = lengthLine;

            Rectangle = rectangle;
            ImageText = new Bitmap(Rectangle.Width, Rectangle.Height);
            formatText = new StringFormat();
            font = new Font("", 0.25f * Rectangle.Height, GraphicsUnit.Pixel);
            g = Graphics.FromImage(ImageText);

            formatText.Alignment = StringAlignment.Center;
            formatText.LineAlignment = StringAlignment.Center;
        }

        protected virtual void DrawText()
        {
            g.Clear(Color.Transparent);

            if (alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    g.DrawString(text, font, brush, Rectangle, formatText);
        }

        public virtual void Reset()
        {
            alpha = 0;
            g.Clear(Color.Transparent);
        }
    }
}
