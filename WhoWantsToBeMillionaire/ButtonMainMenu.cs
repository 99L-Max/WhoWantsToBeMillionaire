using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public readonly MainMenuCommand Command;

        public ButtonMainMenu(MainMenuCommand cmd, float sizeFont) : base()
        {
            Command = cmd;
            Font = new Font("", sizeFont, FontStyle.Bold);
            Dock = DockStyle.Fill;
            OnMouseLeave(EventArgs.Empty);
        }
    }
}
