using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum ThemeButtonCapsule { Blue, Orange, Green, Gray }

    class ButtonСapsule : PictureBox
    {
        private static readonly ReadOnlyDictionary<ThemeButtonCapsule, Image> s_imageButton;

        private ThemeButtonCapsule _theme;
        private Color _foreColor;

        static ButtonСapsule() =>
            s_imageButton = Painter.GetThemeImages<ThemeButtonCapsule>(Resources.ButtonCapsule);

        public ButtonСapsule()
        {
            BackColor = Color.Transparent;

            _foreColor = Color.White;
            _theme = ThemeButtonCapsule.Blue;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(s_imageButton[_theme], ClientRectangle);
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (Enabled)
                SetImageAndForeColor(ThemeButtonCapsule.Blue, Color.White);
            else
                SetImageAndForeColor(ThemeButtonCapsule.Gray, Color.Black);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Font?.Dispose();
            Font = FontManager.CreateFont(GameFont.Arial, 0.45f * ClientRectangle.Height, FontStyle.Bold);

            Invalidate();
        }
    }
}
