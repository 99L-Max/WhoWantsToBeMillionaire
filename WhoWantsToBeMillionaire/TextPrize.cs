using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WhoWantsToBeMillionaire
{
    class TextPrize : MovingPictureBox
    {
        private readonly PictureText prize;

        public TextPrize(Size size) : base(size)
        {
            BackgroundImage = ResourceProcessing.GetImage("Question.png");

            Rectangle rect = new Rectangle(0, 0, size.Width, size.Height);
            Font font = new Font("", 0.42f * size.Height);
            StringFormat format = new StringFormat();

            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            prize = new PictureText(rect, font, format);
        }

        public void Reset()
        {
            X = -Width * 3 / 2;
            Image = null;
        }

        private async Task StartAnimationText(int[] alphas)
        {
            foreach (var a in alphas)
            {
                prize.Alpha = a;
                Image = prize.ImageText;
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        public async Task ShowText(string text, int countFrames)
        {
            int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).ToArray();

            prize.Reset();
            prize.Text = text;

            await StartAnimationText(alphas);
        }

        public async Task HideText(int countFrames)
        {
            int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).OrderByDescending(x => x).ToArray();

            await StartAnimationText(alphas);
        }
    }
}
