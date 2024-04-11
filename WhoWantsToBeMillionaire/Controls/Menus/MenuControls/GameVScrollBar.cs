using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameVScrollBar : GameScrollBar
    {
        public GameVScrollBar() : base(ScrollOrientation.VerticalScroll) 
        {
            Size = new Size(15, 100);//default
        }

        protected override void MouseScroll(MouseEventArgs e)
        {
            var value = Maximum * (e.Y - (ThumbSize >> 1)) / (ClientRectangle.Height - ThumbSize);
            Value = Math.Max(0, Math.Min(Maximum, value));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ThumbSize = (int)(0.2f * ClientRectangle.Height);
        }
    }
}
