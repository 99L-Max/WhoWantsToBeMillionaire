using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameCheckBox : CheckBox
    {
        public GameCheckBox(float fontSize)
        {
            ForeColor = Color.White;
            Dock = DockStyle.Fill;
            Font = new Font("", fontSize, FontStyle.Bold);
        }
    }
}
