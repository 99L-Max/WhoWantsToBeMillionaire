using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class Option : ImageAlphaText
    {
        private readonly Rectangle _textRectangle;
        private readonly Rectangle _letterRectangle;
        private readonly StringFormat _letterFormat;

        private Color _textForeColor;
        private Color _letterForeColor;

        public readonly Letter Letter;

        public bool Selected { get; set; }

        public Option(Letter letter, Rectangle rectangle) : base(rectangle)
        {
            Letter = letter;

            _letterRectangle = new Rectangle(0, 0, (int)(0.15f * Rectangle.Width), Rectangle.Height);
            _textRectangle = new Rectangle(_letterRectangle.Width, 0, Rectangle.Width - _letterRectangle.Width, Rectangle.Height);
            _letterFormat = new StringFormat();

            _formatText.Alignment = StringAlignment.Near;
            _formatText.LineAlignment = StringAlignment.Center;

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
            Selected = false;

            _textForeColor = Color.White;
            _letterForeColor = Color.Orange;

            base.Reset();
        }

        public void SetForeColors(Color colorText, Color colorLetter)
        {
            _textForeColor = colorText;
            _letterForeColor = colorLetter;

            DrawText();
        }

        protected override void DrawText()
        {
            _g.Clear(Color.Transparent);

            if (_alpha > 0)
                using (Brush brustText = new SolidBrush(Color.FromArgb(_alpha, _textForeColor)))
                using (Brush brustLetter = new SolidBrush(Color.FromArgb(_alpha, _letterForeColor)))
                {
                    _g.DrawString(_text, _font, brustText, _textRectangle, _formatText);
                    _g.DrawString($"{Letter}:", _font, brustLetter, _letterRectangle, _letterFormat);
                }
        }
    }
}
