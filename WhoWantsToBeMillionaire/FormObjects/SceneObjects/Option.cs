using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class Option : TextBitmap
    {
        private readonly StringFormat formatLetter;
        private readonly Rectangle rectText;
        private readonly Rectangle rectLetter;

        private Color foreColorText;
        private Color foreColorLetter;

        public readonly Letter Letter;

        public bool Enabled;
        public bool Selected;

        public Option(Letter letter, Rectangle rectangle) : base(rectangle)
        {
            Letter = letter;

            rectLetter = new Rectangle(0, 0, (int)(0.15f * Rectangle.Width), Rectangle.Height);
            rectText = new Rectangle(rectLetter.Width, 0, Rectangle.Width - rectLetter.Width, Rectangle.Height);
            formatLetter = new StringFormat();

            formatText.Alignment = StringAlignment.Near;
            formatText.LineAlignment = StringAlignment.Center;

            formatLetter.Alignment = StringAlignment.Far;
            formatLetter.LineAlignment = StringAlignment.Center;
        }

        public override void Reset()
        {
            Selected = false;
            foreColorText = Color.White;
            foreColorLetter = Color.Orange;

            base.Reset();
        }

        public void SetForeColors(Color colorText, Color colorLetter)
        {
            foreColorText = colorText;
            foreColorLetter = colorLetter;

            DrawText();
        }

        protected override void DrawText()
        {
            g.Clear(Color.Transparent);

            if (alpha > 0)
                using (Brush brustText = new SolidBrush(Color.FromArgb(alpha, foreColorText)))
                using (Brush brustLetter = new SolidBrush(Color.FromArgb(alpha, foreColorLetter)))
                {
                    g.DrawString(text, font, brustText, rectText, formatText);
                    g.DrawString($"{Letter}:", font, brustLetter, rectLetter, formatLetter);
                }
        }
    }
}
