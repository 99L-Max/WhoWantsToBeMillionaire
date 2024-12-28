using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class TableImages : PictureBox, IDisposable
    {
        private readonly List<Image> _images;
        private readonly GameVScrollBar _bar;
        private readonly int _distanseRows;

        public TableImages(int distanseRows)
        {
            _images = new List<Image>();
            _bar = new GameVScrollBar();
            _distanseRows = distanseRows;

            _bar.Dock = DockStyle.Right;
            _bar.Scroll += OnScrollBarValueChanged;

            Controls.Add(_bar);
        }

        public int MouseWheelDelta { get; set; } = 5;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bar.Scroll -= OnScrollBarValueChanged;
                _bar.Dispose();

                _images.ForEach(x => x.Dispose());
                _images.Clear();

                Image?.Dispose();
                BackgroundImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnScrollBarValueChanged(object sender, ScrollEventArgs e) =>
            Invalidate();

        protected override void OnPaint(PaintEventArgs e) =>
            e.Graphics.DrawImage(Image, 0, (ClientRectangle.Height - Image.Height) * _bar.Value / _bar.Maximum);

        protected override void OnMouseWheel(MouseEventArgs e) =>
            _bar.ChangeValue(e.Delta > 0 ? -MouseWheelDelta : MouseWheelDelta);

        public void DrawTable()
        {
            var height = _images.Select(x => x.Height).Sum() + _distanseRows * (_images.Count - 1);
            var image = new Bitmap(ClientRectangle.Width, height);
            var y = 0;

            using (var g = Graphics.FromImage(image))
                foreach (var img in _images)
                {
                    g.DrawImage(img, 0, y, img.Width, img.Height);
                    y += img.Height + _distanseRows;
                }

            Image?.Dispose();
            Image = image;
        }

        public void Add(Image image) =>
            _images.Add(image);

        public void AddText(string text, float fontSize, Size size, Color color, TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter)
        {
            var image = new Bitmap(size.Width, size.Height);

            using (var g = Graphics.FromImage(image))
            using (var font = FontManager.CreateFont(GameFont.Arial, fontSize, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, text, font, new Rectangle(0, 0, image.Width, image.Height), color, flags);
                _images.Add(image);
            }
        }
    }
}
