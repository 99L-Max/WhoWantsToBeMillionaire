using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class Option : ImageAlphaText
    {
        private readonly Rectangle _letterRectangle;
        private readonly StringFormat _letterFormat;

        private Color _letterForeColor;

        public LetterOptions Letter { get; }

        public bool IsSelected { get; set; }

        public Option(LetterOptions letter, Rectangle rectangle) : base(rectangle)
        {
            Letter = letter;

            _letterRectangle = new Rectangle(0, 0, (int)(0.15f * PositionRectangle.Width), PositionRectangle.Height);
            TextRectangle = new Rectangle(_letterRectangle.Width, 0, PositionRectangle.Width - _letterRectangle.Width, PositionRectangle.Height);
            _letterFormat = new StringFormat();

            FormatText.Alignment = StringAlignment.Near;
            FormatText.LineAlignment = StringAlignment.Center;

            _letterFormat.Alignment = StringAlignment.Far;
            _letterFormat.LineAlignment = StringAlignment.Center;
        }

        public override void Dispose()
        {
            base.Dispose();
            _letterFormat.Dispose();
        }

        public override void Reset()
        {
            IsSelected = false;

            ForeColor = Color.White;
            _letterForeColor = Color.Orange;

            base.Reset();
        }

        public void SetForeColors(Color colorText, Color colorLetter)
        {
            ForeColor = colorText;
            _letterForeColor = colorLetter;

            DrawText();
        }

        protected override void DrawText()
        {
            G.Clear(Color.Transparent);

            if (Alpha > 0)
            { 
                using (var brustText = new SolidBrush(Color.FromArgb(Alpha, ForeColor)))
                using (var brustLetter = new SolidBrush(Color.FromArgb(Alpha, _letterForeColor)))
                {
                    G.DrawString(Text, Font, brustText, TextRectangle, FormatText);
                    G.DrawString($"{Letter}:", Font, brustLetter, _letterRectangle, _letterFormat);
                }
            }
        }
    }
}