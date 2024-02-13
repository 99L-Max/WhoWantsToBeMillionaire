using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButton
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    class CustomButton : PictureBox, IDisposable
    {
        private static readonly ReadOnlyDictionary<ThemeButton, Bitmap> ImageButton;

        private Rectangle rectangle;
        private ThemeButton theme;
        private Color foreColor;

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                theme = value ? ThemeButton.Blue : ThemeButton.Gray;
                foreColor = value ? Color.White : Color.Black;
                Invalidate();
            }
            get => base.Enabled;
        }

        static CustomButton()
        {
            var img = new Dictionary<ThemeButton, Bitmap>();

            foreach (var key in Enum.GetValues(typeof(ThemeButton)).Cast<ThemeButton>())
                img.Add(key, new Bitmap(ResourceProcessing.GetImage($"Option_{key}.png")));

            ImageButton = new ReadOnlyDictionary<ThemeButton, Bitmap>(img);
        }

        public CustomButton()
        {
            BackColor = Color.Transparent;
            SizeChanged += OnSizeChanged;
        }

        public CustomButton(Size size) : this()
        {
            Size = size;
            Font = new Font("", 0.35f * size.Height, FontStyle.Bold);
            OnMouseLeave(EventArgs.Empty);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            Size sizeImage = ImageButton[ThemeButton.Blue].Size;

            float wfactor = (float)sizeImage.Width / Width;
            float hfactor = (float)sizeImage.Height / Height;

            float resizeFactor = Math.Max(wfactor, hfactor);

            Size sizeRect = new Size((int)(sizeImage.Width / resizeFactor), (int)(sizeImage.Height / resizeFactor));

            int x = (Width - sizeRect.Width) >> 1;
            int y = (Height - sizeRect.Height) >> 1;

            rectangle = new Rectangle(x, y, sizeRect.Width, sizeRect.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(ImageButton[theme], rectangle);
            TextRenderer.DrawText(e.Graphics, Text, Font, rectangle, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButton.Orange;
                foreColor = Color.Black;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButton.Blue;
                foreColor = Color.White;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (Enabled)
            {
                theme = ThemeButton.Green;
                foreColor = Color.Black;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            OnMouseEnter(mevent);
        }
    }
}