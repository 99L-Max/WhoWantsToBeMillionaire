using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAnimationTransition : GameContol, IResettable
    {
        private const int MovingFramesCount = 12;
        private const int AlphaChangeFramesCount = 6;
        private const int ResizeFramesCount = 6;

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

        public void Reset(Modes mode = Modes.Classic)
        {
            _g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task HideImage()
        {
            using (Image img = new Bitmap(_image))
                await HideImage(img);
        }

        public async Task HideImage(Image image)
        {
            var alphas = Enumerable.Range(0, AlphaChangeFramesCount).Select(frameIndex => byte.MaxValue - byte.MaxValue * frameIndex / (AlphaChangeFramesCount - 1));

            using (var frame = new ImageAlpha(image))
            {
                foreach (var alpha in alphas)
                {
                    frame.Alpha = alpha;

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(frame.Image, ClientRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }

            _g.Clear(Color.Transparent);
            Invalidate();
        }

        public async Task ShowImage(Image image, float minRatio = 0.9f)
        {
            var x0 = (int)(-1.5f * _image.Width);
            var axis = Enumerable.Range(0, MovingFramesCount).Select(frameIndex => x0 - x0 / (MovingFramesCount - 1) * frameIndex);
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
            var axis = Enumerable.Range(0, MovingFramesCount).Select(frameIndex => x0 - x0 / (MovingFramesCount - 1) * frameIndex);
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

            var alphas = Enumerable.Range(0, AlphaChangeFramesCount).Select(frameIndex => byte.MaxValue * frameIndex / (AlphaChangeFramesCount - 1));

            foreach (var alpha in alphas)
            {
                _imageAlphaText.Alpha = alpha;
                _g.DrawImage(_imageAlphaText.ImageText, 0, 0, _image.Width, _image.Height);

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_image, ClientRectangle);
        }

        private async Task ShowResizingImage(Image image, float startRatio, float finalRatio)
        {
            var ratios = Enumerable.Range(0, ResizeFramesCount).Select(frameIndex => startRatio + (finalRatio - startRatio) / (ResizeFramesCount - 1) * frameIndex);
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
    }
}