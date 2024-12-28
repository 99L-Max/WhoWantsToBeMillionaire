using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAnimationTransition : GameContol, IReset
    {
        private const int CountFramesMoving = 12;
        private const int CountFramesAlphaChange = 6;
        private const int CountFramesResize = 6;

        private readonly Image _image;
        private readonly Graphics _g;
        private readonly ImageAlphaText _imageAlphaText;

        public BoxAnimationTransition(int width, int height) : base(width, height)
        {
            _image = new Bitmap(width, height);
            _g = Graphics.FromImage(_image);
            _imageAlphaText = new ImageAlphaText(width, height);
        }

        public Font FontText { set => _imageAlphaText.Font = value; }

        protected override void OnPaint(PaintEventArgs e) =>
            e.Graphics.DrawImage(_image, ClientRectangle);

        public void Reset(Mode mode = Mode.Classic)
        {
            _g.Clear(Color.Transparent);
            Invalidate();
        }

        private async Task ShowResizingImage(Image image, float startRatio, float finalRatio)
        {
            var ratios = Enumerable.Range(0, CountFramesResize).Select(i => startRatio + (finalRatio - startRatio) / (CountFramesResize - 1) * i);
            var rectangle = new Rectangle(new Point(), _image.Size);

            foreach (var ratio in ratios)
            {
                _g.Clear(Color.Transparent);
                _g.DrawImage(image, Resizer.ResizeRectangle(rectangle, ratio));

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }
        }

        private void DrawMovedResizedImage(Image image, Rectangle rectangle, int x)
        {
            rectangle.X += x;
            _g.DrawImage(image, rectangle);
        }

        public async Task HideImage()
        {
            using (Image img = new Bitmap(_image))
                await HideImage(img);
        }

        public async Task HideImage(Image image)
        {
            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue - byte.MaxValue * x / (CountFramesAlphaChange - 1));

            using (var frame = new ImageAlpha(image))
                foreach (var a in alphas)
                {
                    frame.Alpha = a;

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(frame.Image, ClientRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }

            _g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image image, float minRatio = 0.9f)
        {
            var x0 = (int)(-1.5f * _image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);
            var rectangle = Resizer.ResizeRectangle(ClientRectangle, minRatio);

            foreach (var x in axis)
            {
                _g.Clear(Color.Transparent);
                DrawMovedResizedImage(image, rectangle, x);

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }

            await ShowResizingImage(image, minRatio, 1f);
        }

        public async Task ShowTransition(Image startImage, Image finalImage, float minRatio = 0.9f)
        {
            var x0 = (int)(-1.5f * _image.Width);
            var axis = Enumerable.Range(0, CountFramesMoving).Select(i => x0 - x0 / (CountFramesMoving - 1) * i);
            var rectangle = Resizer.ResizeRectangle(ClientRectangle, minRatio);

            await ShowResizingImage(startImage, 1f, minRatio);

            foreach (var x in axis)
            {
                _g.Clear(Color.Transparent);

                DrawMovedResizedImage(startImage, rectangle, x - x0);
                DrawMovedResizedImage(finalImage, rectangle, x);

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }

            await ShowResizingImage(finalImage, minRatio, 1f);
        }

        public async Task ShowText(string text)
        {
            _imageAlphaText.Text = text;

            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            foreach (var a in alphas)
            {
                _imageAlphaText.Alpha = a;
                _g.DrawImage(_imageAlphaText.ImageText, 0, 0, _image.Width, _image.Height);

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }
        }
    }
}