using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class Option : PictureText
    {
        private readonly StringFormat formatLabel;
        private readonly Rectangle rectText;
        private readonly Rectangle rectLabel;

        private Color foreColorText;
        private Color foreColorLabel;

        public readonly Letter Letter;

        public bool Enabled;
        public bool Selected;

        public Option(Rectangle rectangle, Letter letter, Font font, StringFormat format) : base(rectangle, font, format)
        {
            Letter = letter;

            rectLabel = new Rectangle(0, 0, (int)(0.15f * Rectangle.Width), Rectangle.Height);
            rectText = new Rectangle(rectLabel.Width, 0, Rectangle.Width - rectLabel.Width, Rectangle.Height);

            formatLabel = new StringFormat();

            formatLabel.Alignment = StringAlignment.Far;
            formatLabel.LineAlignment = StringAlignment.Center;
        }

        public override void Reset()
        {
            Enabled = true;
            Selected = false;
            foreColorText = Color.White;
            foreColorLabel = Color.Orange;

            base.Reset();
        }

        public void SetForeColors(Color colorText, Color colorLetter)
        {
            foreColorText = colorText;
            foreColorLabel = colorLetter;

            DrawText();
        }

        protected override void DrawText()
        {
            using (Graphics g = Graphics.FromImage(ImageText))
            {
                g.Clear(Color.Transparent);

                if (alpha > 0)
                    using (Brush brustText = new SolidBrush(Color.FromArgb(alpha, foreColorText)))
                    using (Brush brustLabel = new SolidBrush(Color.FromArgb(alpha, foreColorLabel)))
                    {
                        g.DrawString(text, font, brustText, rectText, formatText);
                        g.DrawString($"{Letter}:", font, brustLabel, rectLabel, formatLabel);
                    }
            }
        }
    }
}
