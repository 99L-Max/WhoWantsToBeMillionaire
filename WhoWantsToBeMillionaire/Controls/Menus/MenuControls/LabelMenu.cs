using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelMenu : Label
    {
        public LabelMenu(float fontSize, ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            ForeColor = Color.White;
            Dock = DockStyle.Fill;
            Font = new Font("", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            TextAlign = alignment;
        }
    }
}
