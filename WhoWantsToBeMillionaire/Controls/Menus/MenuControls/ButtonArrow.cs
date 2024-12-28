using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum DirectionArrow { Left, Right }

    class ButtonArrow : PictureBox, IDisposable
    {
        private readonly Image _image;
        private readonly Image _imageClick;

        public readonly DirectionArrow DirectionArrow;

        public ButtonArrow(DirectionArrow direction)
        {
            DirectionArrow = direction;
            Dock = direction == DirectionArrow.Left ? DockStyle.Left : DockStyle.Right;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackColor = Color.Transparent;

            using (var sprite = Resources.ButtonArrow)
            {
                _image = Painter.CutSprite(sprite, 2, 2, 0, (int)direction, false);
                _imageClick = Painter.CutSprite(sprite, 2, 2, 1, (int)direction, false);
            }

            OnMouseLeave(EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image.Dispose();
                _imageClick.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e) =>
            Image = _imageClick;

        protected override void OnMouseUp(MouseEventArgs e) =>
            Image = _imageClick;

        protected override void OnMouseDown(MouseEventArgs e) =>
            Image = _image;

        protected override void OnMouseLeave(EventArgs e) =>
            Image = _image;
    }
}
