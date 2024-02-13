using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class PictureText
    {
        protected readonly Graphics g;

        protected readonly StringFormat formatText;
        protected readonly Font font;

        protected int alpha;
        protected string text;

        public readonly Rectangle Rectangle;

        public string Text
        {
            set
            {
                text = value;
                DrawText();
            }

            get => text;
        }

        public int Alpha
        {
            set
            {
                alpha = value;
                DrawText();
            }
        }

        public Bitmap ImageText { private set; get; }

        public PictureText(Rectangle rectangle, Font font, StringFormat format)
        {
            Rectangle = rectangle;
            ImageText = new Bitmap(Rectangle.Width, Rectangle.Height);
            g = Graphics.FromImage(ImageText);

            this.font = font;
            formatText = format;
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
