using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButtonCapsule
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    class ButtonСapsule : PictureBox
    {
        private static readonly ReadOnlyDictionary<ThemeButtonCapsule, Image> imageButton;

        private ThemeButtonCapsule theme;
        private Color foreColor;

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                theme = value ? ThemeButtonCapsule.Blue : ThemeButtonCapsule.Gray;
                foreColor = value ? Color.White : Color.Black;

                Invalidate();
            }
            get => base.Enabled;
        }

        static ButtonСapsule()
        {
            var keys = Enum.GetValues(typeof(ThemeButtonCapsule)).Cast<ThemeButtonCapsule>();
            var img = keys.ToDictionary(k => k, v => ResourceManager.GetImage($"ButtonCapsule_{v}.png"));
            imageButton = new ReadOnlyDictionary<ThemeButtonCapsule, Image>(img);
        }

        protected ButtonСapsule()
        {
            BackColor = Color.Transparent;
            foreColor = Color.White;
            theme = ThemeButtonCapsule.Blue;
        }

        public ButtonСapsule(int width, int height) : this()
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
                theme = ThemeButtonCapsule.Orange;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonCapsule.Blue;
                foreColor = Color.White;

                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Enabled)
            {
                theme = ThemeButtonCapsule.Green;
                foreColor = Color.Black;

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) => OnMouseEnter(e);
    }
}
