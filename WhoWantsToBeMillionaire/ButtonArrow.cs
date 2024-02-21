using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum DirectionArrow
    {
        Left,
        Right
    }

    class ButtonArrow : PictureBox, IDisposable
    {
        private readonly Image image;
        private readonly Image imageClick;

        public readonly DirectionArrow DirectionArrow;

        public ButtonArrow(DirectionArrow directionArrow)
        {
            DirectionArrow = directionArrow;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;

            image = ResourceProcessing.GetImage($"Arrow_{DirectionArrow}.png");
            imageClick = ResourceProcessing.GetImage($"Arrow_{DirectionArrow}_Click.png");

            Image = image;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Image = imageClick;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Image = image;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Image = image;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                image.Dispose();
                imageClick.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
