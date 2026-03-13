using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameVScrollBar : GameScrollBar
    {
        public GameVScrollBar() : base(ScrollOrientation.VerticalScroll) 
        {
            Size = new Size(15, 100);
        }

        protected override void MouseScroll(MouseEventArgs e)
        {
            var value = Maximum * (e.Y - (ThumbSize >> 1)) / (ClientRectangle.Height - ThumbSize);
            Value = GameMath.Clamp(value, 0, Maximum);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ThumbSize = (int)(0.2f * ClientRectangle.Height);
        }
    }
}
