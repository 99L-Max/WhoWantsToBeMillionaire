using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAnimation : GameContol, IReset
    {
        private const int CountFramesMoving = 12;
        private const int CountFramesAlphaChange = 6;
        private const int CountFramesResize = 6;

        private readonly Image image;
        private readonly Graphics g;
        private readonly BitmapText bitmapText;

        public float SizeFont
        {
            set => bitmapText.SizeFont = value;
        }

        public BoxAnimation(int width, int height) : base(width, height)
        {
            image = new Bitmap(width, height);
            g = Graphics.FromImage(image);
            bitmapText = new BitmapText(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(image, ClientRectangle);
        }

        public void Reset(Mode? mode = null)
        {
            g.Clear(Color.Transparent);
            Invalidate();
        }

        private async Task DrawResizingImage(Image img, float startShare, float finalShare)
        {
            var shares = Enumerable.Range(0, CountFramesResize).Select(i => startShare + (finalShare - startShare) / (CountFramesResize - 1) * i);

            int x, y, width, height;

            foreach (var share in shares)
            {
                width = (int)(share * image.Width);
                height = (int)(share * image.Height);

                x = (image.Width - width) >> 1;
                y = (image.Height - height) >> 1;

                g.Clear(Color.Transparent);
                g.DrawImage(img, x, y, width, height);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        private Rectangle ResizeRectangle(float share)
        {
            int width = (int)(share * image.Width);
            int height = (int)(share * image.Height);

            int x = (image.Width - width) >> 1;
            int y = (image.Height - height) >> 1;

            return new Rectangle(x, y, width, height);
        }

        private void DrawMovedResizedImage(Image img, Rectangle rect, int x)
        {
            rect.X += x;
            g.DrawImage(img, rect);
        }

        public async Task HideImage()
        {
            using (Image img = new Bitmap(image))
                await HideImage(img);
        }

        public async Task HideImage(Image img)
        {
            for (int numAlpha = CountFramesAlphaChange - 1; numAlpha > 0; numAlpha--)
            {
                using (ImageAttributes attribute = new ImageAttributes())
                {
                    ColorMatrix matrix = new ColorMatrix { Matrix33 = (numAlpha + 1f) / CountFramesAlphaChange };
                    attribute.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.Clear(Color.Transparent);
                    g.DrawImage(img, ClientRectangle, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attribute);
                }

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image img)
        {
            float startShare = 0.9f;
            float finalShare = 1f;

            int x0 = (int)(-1.5f * image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);

            var rect = ResizeRectangle(startShare);

            foreach (var x in axis)
            {
                g.Clear(Color.Transparent);
                DrawMovedResizedImage(img, rect, x);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await DrawResizingImage(img, startShare, finalShare);
        }

        public async Task ShowTransition(Image startImg, Image finalImg)
        {
            float startShare = 1f;
            float finalShare = 0.9f;

            int x0 = (int)(-1.5f * image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);

            var rect = ResizeRectangle(finalShare);

            await DrawResizingImage(startImg, startShare, finalShare);

            foreach (var x in axis)
            {
                g.Clear(Color.Transparent);

                DrawMovedResizedImage(startImg, rect, x - x0);
                DrawMovedResizedImage(finalImg, rect, x);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await DrawResizingImage(finalImg, finalShare, startShare);
        }

        public async Task ShowText(string text)
        {
            bitmapText.Text = text;

            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            foreach (var a in alphas)
            {
                bitmapText.Alpha = a;
                g.DrawImage(bitmapText.ImageText, 0, 0, image.Width, image.Height);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }
    }
}