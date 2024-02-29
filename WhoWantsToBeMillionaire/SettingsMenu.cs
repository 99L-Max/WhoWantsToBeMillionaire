using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class SettingsMenu : ContextMenu
    {
        private readonly CheckBox[] checkBoxes;

        public SettingsMenu(Size size, GameSettings settings) : base(0, 0)
        {

        }
    }
}
