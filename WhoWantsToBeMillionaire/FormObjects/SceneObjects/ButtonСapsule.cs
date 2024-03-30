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

    class ButtonСapsule : PictureBox, IDisposable
    {
        private static readonly ReadOnlyDictionary<ThemeButtonCapsule, Image> imageButton;

        private ThemeButtonCapsule _theme;
        private Color _foreColor;

        static ButtonСapsule()
        {
            var keys = Enum.GetValues(typeof(ThemeButtonCapsule)).Cast<ThemeButtonCapsule>();
            var img = keys.ToDictionary(k => k, v => ResourceManager.GetImage($"ButtonCapsule_{v}.png"));
            imageButton = new ReadOnlyDictionary<ThemeButtonCapsule, Image>(img);
        }

        protected ButtonСapsule()
        {
            BackColor = Color.Transparent;

            _foreColor = Color.White;
            _theme = ThemeButtonCapsule.Blue;

            EnabledChanged += OnEnabledChanged;
        }

        public ButtonСapsule(int width, int height) : this()
        {
            Size = new Size(width, height);
            Font = new Font("", 0.5f * height, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(imageButton[_theme], ClientRectangle);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, _foreColor);
        }

        private void SetImageAndForeColor(ThemeButtonCapsule theme, Color foreColor)
        {
            _theme = theme;
            _foreColor = foreColor;

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) =>
            SetImageAndForeColor(ThemeButtonCapsule.Orange, Color.Black);

        protected override void OnMouseLeave(EventArgs e) =>
            SetImageAndForeColor(ThemeButtonCapsule.Blue, Color.White);

        protected override void OnMouseDown(MouseEventArgs e) =>
            SetImageAndForeColor(ThemeButtonCapsule.Green, Color.Black);

        protected override void OnMouseUp(MouseEventArgs e) =>
            SetImageAndForeColor(ThemeButtonCapsule.Orange, Color.Black);

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
                SetImageAndForeColor(ThemeButtonCapsule.Blue, Color.White);
            else
                SetImageAndForeColor(ThemeButtonCapsule.Gray, Color.Black);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                EnabledChanged -= OnEnabledChanged;

            base.Dispose(disposing);
        }
    }
}
