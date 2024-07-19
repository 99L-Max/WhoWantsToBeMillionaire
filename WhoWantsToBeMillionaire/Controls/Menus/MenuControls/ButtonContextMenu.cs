using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonContextMenu : ButtonСapsule
    {
        public readonly ContextMenuCommand Command;

        public ButtonContextMenu(ContextMenuCommand command) : base()
        {
            Command = command;
        }

        public void AlignSize(float relativeWidth, float relativeHeight)
        {
            Dock = DockStyle.Fill;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;

            SizeF ratio = new SizeF(relativeWidth, relativeHeight);

            float wFactor = ratio.Width / ClientRectangle.Width;
            float hFactor = ratio.Height / ClientRectangle.Height;

            float resizeFactor = Math.Max(wFactor, hFactor);

            Dock = DockStyle.None;
            Anchor = AnchorStyles.None;
            Size = new Size((int)(ratio.Width / resizeFactor), (int)(ratio.Height / resizeFactor));
        }
    }
}
