using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameContol : PictureBox
    {
        public GameContol(Size size)
        {
            Size = size;
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public GameContol(int width, int height) : this(new Size(width, height)) { }
    }
}
