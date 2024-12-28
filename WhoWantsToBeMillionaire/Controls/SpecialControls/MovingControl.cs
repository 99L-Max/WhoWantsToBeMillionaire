using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class MovingControl : GameContol
    {
        public MovingControl(int width, int height) : base(width, height) { }

        public MovingControl(Size size) : base(size) { }

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
            get => Location.X;
            set => Location = new Point(value, Location.Y);
        }

        public int Y
        {
            get => Location.Y;
            set => Location = new Point(Location.X, value);
        }

        public async Task MoveX(int x, int countFrames)
        {
            int dx = (x - X) / countFrames;

            do
            {
                X += dx;
                await Task.Delay(GameConst.DeltaTime);
            } while (--countFrames > 0);

            X = x;
        }

        public async Task MoveY(int y, int countFrames)
        {
            int dy = (y - Y) / countFrames;

            do
            {
                Y += dy;
                await Task.Delay(GameConst.DeltaTime);
            } while (--countFrames > 0);

            Y = y;
        }
    }
}
