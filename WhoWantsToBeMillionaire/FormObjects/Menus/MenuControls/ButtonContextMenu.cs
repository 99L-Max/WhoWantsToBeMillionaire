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
            Dock = DockStyle.Fill;
            Font = new Font("", fontSize, FontStyle.Bold);
        }
    }
}
