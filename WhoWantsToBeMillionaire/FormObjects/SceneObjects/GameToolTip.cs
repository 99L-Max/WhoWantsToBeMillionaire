using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameToolTip : ToolTip, IDisposable
    {
        public int Border { set; get; } = 3;

        public Size Size { set; get; }

        public float FontSize { set; get; }

        public GameToolTip(int width, int height, float fontSize)
        {
            Size = new Size(width, height);
            FontSize = fontSize;

            OwnerDraw = true;
            Popup += OnPopup;
            Draw += OnDraw;
        }

        private void OnPopup(object sender, PopupEventArgs e) =>
            e.ToolTipSize = Size;

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            Rectangle rectFrame = new Rectangle(0, 0, Size.Width, Size.Height);
            Rectangle rectFill = new Rectangle(Border, Border, rectFrame.Width - 2 * Border, rectFrame.Height - 2 * Border);

            using (Font font = new Font("", FontSize, FontStyle.Bold, GraphicsUnit.Point))
            using (LinearGradientBrush brushFrame = new LinearGradientBrush(rectFrame, Color.Gainsboro, Color.SlateGray, 45f))
            using (LinearGradientBrush brushFill = new LinearGradientBrush(rectFill, Color.Navy, Color.Black, 90f))
            {
                e.Graphics.FillRectangle(brushFrame, rectFrame);
                e.Graphics.FillRectangle(brushFill, rectFill);
                e.Graphics.DrawString(e.ToolTipText, font, Brushes.White, rectFill);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Popup -= OnPopup;
                Draw -= OnDraw;
            }

            base.Dispose(disposing);
        }
    }
}
