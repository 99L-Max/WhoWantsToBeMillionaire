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

        private readonly Image _image;
        private readonly Graphics _g;
        private readonly TextBitmap _textBitmap;

        public BoxAnimation(int width, int height) : base(width, height)
        {
            _image = new Bitmap(width, height);
            _g = Graphics.FromImage(_image);
            _textBitmap = new TextBitmap(width, height);
        }

        public float SizeFont { set => _textBitmap.SizeFont = value; }

        protected override void OnPaint(PaintEventArgs e) => 
            e.Graphics.DrawImage(_image, ClientRectangle);

        public void Reset(Mode mode = Mode.Classic)
        {
            _g.Clear(Color.Transparent);
            Invalidate();
        }

        private async Task DrawResizingImage(Image img, float startShare, float finalShare)
        {
            var shares = Enumerable.Range(0, CountFramesResize).Select(i => startShare + (finalShare - startShare) / (CountFramesResize - 1) * i);

            int x, y, width, height;

            foreach (var share in shares)
            {
                width = (int)(share * _image.Width);
                height = (int)(share * _image.Height);

                x = _image.Width - width >> 1;
                y = _image.Height - height >> 1;

                _g.Clear(Color.Transparent);
                _g.DrawImage(img, x, y, width, height);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        private Rectangle ResizeRectangle(float share)
        {
            int width = (int)(share * _image.Width);
            int height = (int)(share * _image.Height);

            int x = _image.Width - width >> 1;
            int y = _image.Height - height >> 1;

            return new Rectangle(x, y, width, height);
        }

        private void DrawMovedResizedImage(Image img, Rectangle rect, int x)
        {
            rect.X += x;
            _g.DrawImage(img, rect);
        }

        public async Task HideImage()
        {
            using (Image img = new Bitmap(_image))
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

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(img, ClientRectangle, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attribute);
                }

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            _g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image img)
        {
            var startShare = 0.9f;
            var finalShare = 1f;

            var x0 = (int)(-1.5f * _image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);

            var rect = ResizeRectangle(startShare);

            foreach (var x in axis)
            {
                _g.Clear(Color.Transparent);
                DrawMovedResizedImage(img, rect, x);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await DrawResizingImage(img, startShare, finalShare);
        }

        public async Task ShowTransition(Image startImg, Image finalImg)
        {
            var startShare = 1f;
            var finalShare = 0.9f;

            var x0 = (int)(-1.5f * _image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);

            var rect = ResizeRectangle(finalShare);

            await DrawResizingImage(startImg, startShare, finalShare);

            foreach (var x in axis)
            {
                _g.Clear(Color.Transparent);

                DrawMovedResizedImage(startImg, rect, x - x0);
                DrawMovedResizedImage(finalImg, rect, x);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await DrawResizingImage(finalImg, finalShare, startShare);
        }

        public async Task ShowText(string text)
        {
            _textBitmap.Text = text;

            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            foreach (var a in alphas)
            {
                _textBitmap.Alpha = a;
                _g.DrawImage(_textBitmap.ImageText, 0, 0, _image.Width, _image.Height);

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }
    }
}