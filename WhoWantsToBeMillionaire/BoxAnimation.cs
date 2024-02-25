using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAnimation : GameContol
    {
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

        public async Task HideImage(int countFrames)
        {
            using (Image img = new Bitmap(mainImage))
                for (int i = countFrames - 1; i > 0; i--)
                {
                    g.Clear(Color.Transparent);
                    DrawFrame(img, i, countFrames);
                    await Task.Delay(MainForm.DeltaTime);
                }

            g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task HideImage(Image image, int countFrames)
        {
            for (int i = countFrames - 1; i > 0; i--)
            {
                g.Clear(Color.Transparent);
                DrawFrame(image, i, countFrames);
                await Task.Delay(MainForm.DeltaTime);
            }

            g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image image, int framesTrans, int framesResize)
        {
            float share = 0.9f;
            float dShare = (1f - share) / (framesResize - 1);

            int x0 = (int)(-1.5f * mainImage.Width);
            int dx = -x0 / (framesTrans - 1);

            for (int x = x0; x < 0; x += dx)
            {
                g.Clear(Color.Transparent);
                DrawResizedImage(image, x, share);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            for (int i = 0; i < framesResize; i++)
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

        public async Task ShowTransition(Image startImg, Image finalImg, int framesTrans, int framesResize)
        {
            float share = 1f;
            float dShare = 0.1f / (framesResize - 1);

            for (int i = 0; i < framesResize; i++)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(startImg, 0, share);
                share -= dShare;

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            int x0 = (int)(-1.5f * mainImage.Width);
            int dx = -x0  / (framesTrans - 1);

            for (int x = x0; x < 0; x += dx)
            {
                g.Clear(Color.Transparent);

                DrawResizedImage(startImg, x - x0, share);
                DrawResizedImage(finalImg, x, share);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            for (int i = 0; i < framesResize; i++)
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

        public async Task ShowText(string text, int countFrames)
        {
            bitmapText.Text = text;

            int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).ToArray();

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
