using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WhoWantsToBeMillionaire
{
    class ImageAlpha : IDisposable
    {
        private readonly Rectangle _regtangle;
        private readonly Graphics _g;
        private readonly Image _sample;
        private readonly ImageAttributes _attribute;

        private int _alpha = 0;

        public ImageAlpha(Image image)
        {
            Image = new Bitmap(image.Width, image.Height);

            _sample = new Bitmap(image);
            _regtangle = new Rectangle(0, 0, image.Width, image.Height);
            _g = Graphics.FromImage(Image);
            _attribute = new ImageAttributes();

            _g.CompositingMode = CompositingMode.SourceCopy;
        }

        public Image Image { get; private set; }

        public int Alpha
        {
            get => _alpha;
            set => SetAlpha(value);
        }

        public void Dispose()
        {
            _sample.Dispose();
            _g.Dispose();
            _attribute.Dispose();

            Image.Dispose();
        }

        private void SetAlpha(int alpha)
        {
            if (_alpha != alpha)
            {
                _alpha = alpha;

                var matrix = new ColorMatrix { Matrix33 = (_alpha + 1f) / 255f };

                _attribute.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                _g.DrawImage(_sample, _regtangle, 0, 0, _sample.Width, _sample.Height, GraphicsUnit.Pixel, _attribute);
            }
        }

        public virtual void Reset()
        {
            _alpha = 0;
            _g.Clear(Color.Transparent);
        }
    }
}