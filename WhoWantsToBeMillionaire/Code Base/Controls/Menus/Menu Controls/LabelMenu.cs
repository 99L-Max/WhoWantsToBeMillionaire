using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelMenu : Label
    {
        public LabelMenu(float fontSize, ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            ForeColor = Color.White;
            Font = FontFactory.CreateFont(GameFonts.Arial, fontSize, FontStyle.Bold);
            TextAlign = alignment;
        }
    }
}