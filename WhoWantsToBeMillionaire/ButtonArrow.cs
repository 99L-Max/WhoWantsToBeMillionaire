using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum DirectionArrow
    {
        Left,
        Right
    }

    class ButtonArrow : PictureBox
    {
        private readonly string arrow;

        public readonly DirectionArrow DirectionArrow;

        public ButtonArrow(DirectionArrow directionArrow, float fontSize)
        {
            DirectionArrow = directionArrow;
            BackColor = Color.Transparent;
            Font = new Font(FontFamily.GenericMonospace, fontSize, FontStyle.Bold);
            arrow = directionArrow == DirectionArrow.Left ? "<" : ">";

            OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, arrow, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e) => ForeColor = Color.Orange;

        protected override void OnMouseUp(MouseEventArgs e) => ForeColor = Color.Orange;

        protected override void OnMouseDown(MouseEventArgs e) => ForeColor = Color.Indigo;

        protected override void OnMouseLeave(EventArgs e) => ForeColor = Color.Indigo;
    }
}
