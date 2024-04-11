using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonContextMenu : ButtonСapsule
    {
        public readonly ContextMenuCommand Command;

        public ButtonContextMenu(ContextMenuCommand cmd) : base()
        {
            Command = cmd;
        }

        public void AlignSize(float relativeWidth, float relativeHeight)
        {
            Dock = DockStyle.Fill;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;

            SizeF ratio = new SizeF(relativeWidth, relativeHeight);

            float wfactor = ratio.Width / ClientRectangle.Width;
            float hfactor = ratio.Height / ClientRectangle.Height;

            float resizeFactor = Math.Max(wfactor, hfactor);

            Dock = DockStyle.None;
            Anchor = AnchorStyles.None;
            Size = new Size((int)(ratio.Width / resizeFactor), (int)(ratio.Height / resizeFactor));
        }
    }
}
