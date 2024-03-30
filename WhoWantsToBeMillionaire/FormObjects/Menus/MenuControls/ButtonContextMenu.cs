using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonContextMenu : ButtonСapsule
    {
        public readonly ContextMenuCommand Command;

        public ButtonContextMenu(ContextMenuCommand cmd, float fontSize) : base()
        {
            Command = cmd;
            Font = new Font("", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
           Size sizeImage = new Size(6, 1);
           
           float wfactor = (float)sizeImage.Width / ClientRectangle.Width;
           float hfactor = (float)sizeImage.Height / ClientRectangle.Height;
           
           float resizeFactor = Math.Max(wfactor, hfactor);

            Size = new Size((int)(sizeImage.Width / resizeFactor), (int)(sizeImage.Height / resizeFactor));
        }
    }
}
