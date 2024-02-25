using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAnimation : GameContol
    {
        private const int CountFramesMoving = 15;
        private const int CountFramesAlphaChange = 6;
        private const int CountFramesResize = 6;

        private readonly Image mainImage;
        private readonly Graphics g;
        private readonly BitmapText bitmapText;

        public float SizeFont
        {
            set => bitmapText.SizeFont = value;
        }

        public BoxAnimation(int width, int height) : base(width, height)
        {
            mainImage = new Bitmap(width, height);
            g = Graphics.FromImage(mainImage);
            bitmapText = new BitmapText(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(mainImage, ClientRectangle);
        }

        private void DrawFrame(Image frame, int numberFrame, int countFrames)
        {
            using (ImageAttributes attribute = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix { Matrix33 = (numberFrame + 1f) / countFrames };
                attribute.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(frame, ClientRectangle, 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel, attribute);
            }

            Invalidate();
        }

        private void DrawResizedImage(Image image, int xImg, float share)
        {
            int width = (int)(share * mainImage.Width);
            int height = (int)(share * mainImage.Height);

            int x = ((mainImage.Width - width) >> 1) + xImg;
            int y = (mainImage.Height - height) >> 1;

            g.DrawImage(image, x, y, (int)(share * mainImage.Width), (int)(share * mainImage.Height));
        }

        public async Task HideImage()
        {
            using (Image img = new Bitmap(mainImage))
                await HideImage(img);
        }

        public async Task HideImage(Image image)
        {
            for (int i = CountFramesAlphaChange - 1; i > 0; i--)
            {
                g.Clear(Color.Transparent);
                DrawFrame(image, i, CountFramesAlphaChange);
                await Task.Delay(MainForm.DeltaTime);
            }

            g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image image)
        {
            float share = 0.9f;
            float dShare = (1f - share) / (CountFramesResize - 1);

            int x0 = (int)(-1.5f * mainImage.Width);
            int dx = -x0 / (CountFramesMoving - 1);

            for (int x = x0; x < 0; x += dx)
            {
                g.Clear(Color.Transparent);
                DrawResizedImage(image, x, share);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            for (int i = 0; i < CountFramesResize; i++)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(image, 0, share);
                share += dShare;

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            g.DrawImage(image, 0, 0, mainImage.Width, mainImage.Height);
            Invalidate();
        }

        public async Task ShowTransition(Image startImg, Image finalImg)
        {
            float share = 1f;
            float dShare = 0.1f / (CountFramesResize - 1);

            for (int i = 0; i < CountFramesResize; i++)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(startImg, 0, share);
                share -= dShare;

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            int x0 = (int)(-1.5f * mainImage.Width);
            int dx = -x0  / (CountFramesMoving - 1);

            for (int x = x0; x < 0; x += dx)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(startImg, x - x0, share);
                DrawResizedImage(finalImg, x, share);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            for (int i = 0; i < CountFramesResize; i++)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(finalImg, 0, share);
                share += dShare;

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            g.DrawImage(finalImg, 0, 0, mainImage.Width, mainImage.Height);
            Invalidate();
        }

        public async Task ShowText(string text)
        {
            bitmapText.Text = text;

            int[] alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1)).ToArray();

            foreach (var a in alphas)
            {
                bitmapText.Alpha = a;
                g.DrawImage(bitmapText.ImageText, 0, 0, mainImage.Width, mainImage.Height);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }
    }
}
