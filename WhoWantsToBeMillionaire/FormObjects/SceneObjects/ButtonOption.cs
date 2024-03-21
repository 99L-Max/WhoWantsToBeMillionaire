using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonOption : Label
    {
        private static readonly Image image;

        private bool imageVisible;

        private bool ImageVisible
        {
            set
            {
                if(imageVisible != value)
                {
                    imageVisible = value;
                    Invalidate();
                }
            }
        }

        public readonly Letter Letter;

        static ButtonOption() =>
            image = ResourceManager.GetImage("ButtonWire_Focused.png");

        public ButtonOption(Letter letter, Rectangle rectangle)
        {
            Letter = letter;
            Location = rectangle.Location;
            Size = rectangle.Size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (imageVisible)
                e.Graphics.DrawImage(image, ClientRectangle);
        }

        protected override void OnMouseEnter(EventArgs e) => ImageVisible = true;

        protected override void OnMouseLeave(EventArgs e) => ImageVisible = false;
    }
}
