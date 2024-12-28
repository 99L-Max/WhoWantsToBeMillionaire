using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ButtonOption : Label
    {
        private static readonly Image s_image;

        private bool _imageVisible;

        public readonly LetterOption Letter;

        static ButtonOption() =>
            s_image = Resources.ButtonWire_Focused;

        public ButtonOption(LetterOption letter, Rectangle rectangle)
        {
            Letter = letter;
            Location = rectangle.Location;
            Size = rectangle.Size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_imageVisible)
                e.Graphics.DrawImage(s_image, ClientRectangle);
        }

        private void ChangeImageVisible(bool visible)
        {
            _imageVisible = visible;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) => 
            ChangeImageVisible(true);

        protected override void OnMouseLeave(EventArgs e) => 
            ChangeImageVisible(false);
    }
}
