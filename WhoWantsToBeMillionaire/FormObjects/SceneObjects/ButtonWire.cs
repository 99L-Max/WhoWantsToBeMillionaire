using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButtonWire
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    class ButtonWire : PictureBox, IDisposable
    {
        private static readonly ReadOnlyDictionary<ThemeButtonWire, Image> imageButton;
        private static readonly Image wire;

        private readonly Label leftBarrier;
        private readonly Label rightBarrier;

        private Rectangle rectImage;
        private Rectangle rectBackground;
        private Color foreColor;
        private ThemeButtonWire theme;

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                theme = value ? ThemeButtonWire.Blue : ThemeButtonWire.Gray;
                foreColor = value ? Color.White : Color.Black;

                Invalidate();
            }
            get => base.Enabled;
        }

        static ButtonWire()
        {
            var keys = Enum.GetValues(typeof(ThemeButtonWire)).Cast<ThemeButtonWire>();
            var img = keys.ToDictionary(k => k, v => ResourceManager.GetImage($"ButtonWire_{v}.png"));

            imageButton = new ReadOnlyDictionary<ThemeButtonWire, Image>(img);
            wire = ResourceManager.GetImage("Wire.png");
        }

        public ButtonWire(float sizeFont)
        {
            BackColor = Color.Transparent;
            Dock = DockStyle.Fill;
            Font = new Font("", sizeFont, FontStyle.Bold);

            foreColor = Color.White;
            theme = ThemeButtonWire.Blue;

            leftBarrier = new Label();
            rightBarrier = new Label();

            Controls.Add(leftBarrier);
            Controls.Add(rightBarrier);

            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            Size sizeImage = imageButton[ThemeButtonWire.Blue].Size;

            float wfactor = (float)sizeImage.Width / Width;
            float hfactor = (float)sizeImage.Height / Height;

            float resizeFactor = Math.Max(wfactor, hfactor);

            Size sizeRect = new Size((int)(sizeImage.Width / resizeFactor), (int)(sizeImage.Height / resizeFactor));

            int x = (Width - sizeRect.Width) >> 1;
            int y = (Height - sizeRect.Height) >> 1;

            rectImage = new Rectangle(x, y, sizeRect.Width, sizeRect.Height);
            rectBackground = new Rectangle(0, y, Width, sizeRect.Height);

            rightBarrier.Size = leftBarrier.Size = new Size((Width - rectImage.Width) >> 1, Height);
            rightBarrier.Location = new Point(Width - rightBarrier.Width, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(wire, rectBackground);
            e.Graphics.DrawImage(imageButton[theme], rectImage);
            TextRenderer.DrawText(e.Graphics, Text, Font, rectImage, foreColor);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonWire.Orange;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonWire.Blue;
                foreColor = Color.White;

                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonWire.Green;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) => OnMouseEnter(e);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SizeChanged -= OnSizeChanged;

                leftBarrier.Dispose();
                rightBarrier.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
