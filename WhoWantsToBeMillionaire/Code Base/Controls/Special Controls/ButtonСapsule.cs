using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ButtonСapsule : PictureBox
    {
        private static readonly ReadOnlyDictionary<ButtonCapsuleThemes, Image> s_imageButton;

        private ButtonCapsuleThemes _theme;
        private Color _foreColor;

        static ButtonСapsule()
        { 
            s_imageButton = SpritePainter.GetEnumSpritesList<ButtonCapsuleThemes>(Resources.ButtonCapsule);
        }

        public ButtonСapsule()
        {
            BackColor = Color.Transparent;

            _foreColor = Color.White;
            _theme = ButtonCapsuleThemes.Blue;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(s_imageButton[_theme], ClientRectangle);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, _foreColor);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            SetImageAndForeColor(ButtonCapsuleThemes.Orange, Color.Black);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            SetImageAndForeColor(ButtonCapsuleThemes.Blue, Color.White);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SetImageAndForeColor(ButtonCapsuleThemes.Green, Color.Black);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            SetImageAndForeColor(ButtonCapsuleThemes.Orange, Color.Black);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (Enabled)
                SetImageAndForeColor(ButtonCapsuleThemes.Blue, Color.White);
            else
                SetImageAndForeColor(ButtonCapsuleThemes.Gray, Color.Black);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Font?.Dispose();
            Font = FontFactory.CreateFont(GameFonts.Arial, 0.45f * ClientRectangle.Height, FontStyle.Bold);
        }

        private void SetImageAndForeColor(ButtonCapsuleThemes theme, Color foreColor)
        {
            _theme = theme;
            _foreColor = foreColor;

            Invalidate();
        }
    }
}
