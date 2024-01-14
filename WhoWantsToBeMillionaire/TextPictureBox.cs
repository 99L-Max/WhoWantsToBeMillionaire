using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class TextPictureBox : PictureBox
    {
        protected readonly StringFormat format;

        protected int alpha;

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

        public TextPictureBox(Size size, Bitmap backgroundImage, float fontSize, StringFormat stringFormat)
        {
            Size = size;
            format = stringFormat;

            SizeMode = PictureBoxSizeMode.StretchImage;
            BackgroundImageLayout = ImageLayout.Stretch;

            ForeColor = Color.White;
            Font = new Font("", fontSize);
            BackgroundImage = backgroundImage;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, ForeColor)))
                    e.Graphics.DrawString(Text, Font, brush, ClientRectangle, format);
        }

        private async Task StartAnimationText(bool isShow, int countFrames)
        {
            int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).ToArray();

            if (!isShow)
                Array.Reverse(alphas);

            foreach (var a in alphas)
            {
                alpha = a;
                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        public async Task ShowText(int countFrames)
        {
            await StartAnimationText(true, countFrames);
        }

        public async Task HideText(int countFrames)
        {
            await StartAnimationText(false, countFrames);
        }
    }
}
