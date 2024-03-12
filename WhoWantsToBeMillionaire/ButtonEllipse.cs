using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButtonEllipse
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    class ButtonEllipse : PictureBox
    {
        private static readonly ReadOnlyDictionary<ThemeButtonEllipse, Bitmap> imageButton;

        private ThemeButtonEllipse theme;
        private Color foreColor;

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                theme = value ? ThemeButtonEllipse.Blue : ThemeButtonEllipse.Gray;
                foreColor = value ? Color.White : Color.Black;

                Invalidate();
            }
            get => base.Enabled;
        }

        static ButtonEllipse()
        {
            var img = new Dictionary<ThemeButtonEllipse, Bitmap>();

            foreach (var key in Enum.GetValues(typeof(ThemeButtonEllipse)).Cast<ThemeButtonEllipse>())
                img.Add(key, new Bitmap(ResourceProcessing.GetImage($"ButtonEllipse_{key}.png")));

            imageButton = new ReadOnlyDictionary<ThemeButtonEllipse, Bitmap>(img);
        }

        protected ButtonEllipse()
        {
            BackColor = Color.Transparent;
            foreColor = Color.White;
            theme = ThemeButtonEllipse.Blue;
        }

        public ButtonEllipse(int width, int height) : this()
        {
            Size = new Size(width, height);
            Font = new Font("", 0.35f * height, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(imageButton[theme], ClientRectangle);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, foreColor);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonEllipse.Orange;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonEllipse.Blue;
                foreColor = Color.White;

                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonEllipse.Green;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnMouseEnter(e);
        }
    }
}
