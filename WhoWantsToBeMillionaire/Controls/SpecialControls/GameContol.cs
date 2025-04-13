using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class GameContol : PictureBox
    {
        private GameContol()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public GameContol(Size size) : this()
        {
            Size = size;
        }

        public GameContol(int width, int height) : this()
        {
            Size = new Size(width, height);
        }

        public GameContol(BasicSize basicSize, int size, int widthFraction, int heightFraction) : this()
        {
            Size = Resizer.Resize(basicSize, size, widthFraction, heightFraction);
        }
    }
}
