﻿using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class Option : ImageAlphaText
    {
        private readonly Rectangle _letterRectangle;
        private readonly StringFormat _letterFormat;

        private Color _letterForeColor;

        public readonly LetterOption Letter;

        public bool Selected { get; set; }

        public Option(LetterOption letter, Rectangle rectangle) : base(rectangle)
        {
            Letter = letter;

            _letterRectangle = new Rectangle(0, 0, (int)(0.15f * PositionRectangle.Width), PositionRectangle.Height);
            _textRectangle = new Rectangle(_letterRectangle.Width, 0, PositionRectangle.Width - _letterRectangle.Width, PositionRectangle.Height);
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

            _foreColor = Color.White;
            _letterForeColor = Color.Orange;

            base.Reset();
        }

        public void SetForeColors(Color colorText, Color colorLetter)
        {
            _foreColor = colorText;
            _letterForeColor = colorLetter;

            DrawText();
        }

        protected override void DrawText()
        {
            _g.Clear(Color.Transparent);

            if (_alpha > 0)
                using (var brustText = new SolidBrush(Color.FromArgb(_alpha, _foreColor)))
                using (var brustLetter = new SolidBrush(Color.FromArgb(_alpha, _letterForeColor)))
                {
                    _g.DrawString(_text, _font, brustText, _textRectangle, _formatText);
                    _g.DrawString($"{Letter}:", _font, brustLetter, _letterRectangle, _letterFormat);
                }
        }
    }
}