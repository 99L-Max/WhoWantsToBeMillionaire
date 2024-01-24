using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class MovingPictureBox : PictureBox
    {
        public int X
        {
            set => Location = new Point(value, Location.Y);
            get => Location.X;
        }

        public int Y
        {
            set => Location = new Point(Location.X, value);
            get => Location.Y;
        }

        public MovingPictureBox(Size size)
        {
            Size = size;
            SizeMode = PictureBoxSizeMode.StretchImage;
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        public async Task MoveX(int x, int countFrames)
        {
            int dx = (x - X) / countFrames;

            do
            {
                X += dx;
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            X = x;
        }
    }
}
