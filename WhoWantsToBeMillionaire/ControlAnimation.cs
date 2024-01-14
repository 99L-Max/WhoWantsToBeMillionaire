using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class ControlAnimation : PictureBox
    {
        public ControlAnimation(Size size)
        {
            Size = size;
            BackColor = Color.Transparent;
        }
    }
}
