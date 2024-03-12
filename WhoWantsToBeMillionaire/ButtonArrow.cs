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

        public ButtonArrow(DirectionArrow direction)
        {
            DirectionArrow = direction;

            image = ResourceProcessing.GetImage($"ButtonArrow_{direction}.png");
            imageClick = ResourceProcessing.GetImage($"ButtonArrow_{direction}_Click.png");

            OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e) => e.Graphics.DrawImage(Image, ClientRectangle);

        protected override void OnMouseEnter(EventArgs e) => Image = imageClick;

        protected override void OnMouseUp(MouseEventArgs e) => Image = imageClick;

        protected override void OnMouseDown(MouseEventArgs e) => Image = image;

        protected override void OnMouseLeave(EventArgs e) => Image = image;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image = null;

                image.Dispose();
                imageClick.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
