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
        public static readonly ReadOnlyDictionary<ThemeButton, Bitmap> ImageButton;

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                BackgroundImage = ImageButton[value ? ThemeButton.Blue : ThemeButton.Gray];
                ForeColor = value ? Color.White : Color.Black;
            }
            get => base.Enabled;
        }

        static CustomButton()
        {
            var img = new Dictionary<ThemeButton, Bitmap>();

            foreach (var key in Enum.GetValues(typeof(ThemeButton)).Cast<ThemeButton>())
                img.Add(key, new Bitmap(ResourceProcessing.GetImage($"Answer_{key}.png")));

            ImageButton = new ReadOnlyDictionary<ThemeButton, Bitmap>(img);
        }

        public CustomButton()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Zoom;
        }

        public CustomButton(Size size) : this()
        {
            Size = size;
            Font = new Font("", 0.35f * size.Height, FontStyle.Bold);
            OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled)
            {
                BackgroundImage = ImageButton[ThemeButton.Orange];
                ForeColor = Color.Black;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                BackgroundImage = ImageButton[ThemeButton.Blue];
                ForeColor = Color.White;
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (Enabled)
                BackgroundImage = ImageButton[ThemeButton.Green];
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            OnMouseEnter(mevent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackgroundImage.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
