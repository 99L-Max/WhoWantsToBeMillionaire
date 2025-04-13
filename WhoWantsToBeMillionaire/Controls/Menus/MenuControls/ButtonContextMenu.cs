using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonContextMenu : ButtonСapsule, IAlignSize
    {
        public readonly ContextMenuCommand Command;

        public ButtonContextMenu(ContextMenuCommand command) : base()
        {
            Command = command;
        }

        public void AlignSize()
        {
            Dock = DockStyle.Fill;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;

            SizeF ratio = new SizeF(6, 1);

            float wFactor = ratio.Width / ClientRectangle.Width;
            float hFactor = ratio.Height / ClientRectangle.Height;

            float resizeFactor = Math.Max(wFactor, hFactor);

            Dock = DockStyle.None;
            Anchor = AnchorStyles.None;
            Size = new Size((int)(ratio.Width / resizeFactor), (int)(ratio.Height / resizeFactor));
        }
    }
}
