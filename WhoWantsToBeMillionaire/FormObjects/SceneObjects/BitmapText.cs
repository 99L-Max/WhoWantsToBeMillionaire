using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class BitmapText
    {
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
                font = new Font("", value);
                DrawText();
            }
        }

        public string Text
        {
            set
            {
                text = value;

                if (alpha > 0)
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

        public Bitmap ImageText { private set; get; }

        public BitmapText(int width, int height) : this(new Rectangle(0, 0, width, height)) { }

        public BitmapText(Rectangle rectangle)
        {
            Rectangle = rectangle;
            ImageText = new Bitmap(Rectangle.Width, Rectangle.Height);
            formatText = new StringFormat();
            font = new Font("", 0.25f * Rectangle.Height);
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
