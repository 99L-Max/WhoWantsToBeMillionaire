using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class MovingControl : GameContol
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                base.CreateParams.ExStyle |= 0x02000000;
                return cp;
            }
        }

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

        public MovingControl(int width, int height) : base(width, height) { }

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

        public async Task MoveY(int y, int countFrames)
        {
            int dy = (y - Y) / countFrames;

            do
            {
                Y += dy;
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            Y = y;
        }
    }
}
