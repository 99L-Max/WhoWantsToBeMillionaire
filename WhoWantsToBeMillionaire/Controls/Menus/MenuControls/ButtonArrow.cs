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
            SizeMode = PictureBoxSizeMode.Zoom;
            BackColor = Color.Transparent;

            _image = direction == DirectionArrow.Left ? Resources.ButtonArrow_Left : Resources.ButtonArrow_Right;
            _imageClick = direction == DirectionArrow.Left ? Resources.ButtonArrow_Left_Click : Resources.ButtonArrow_Right_Click;

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
