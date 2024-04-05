using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameToolTip : ToolTip, IDisposable
    {
        private readonly Font _font;
        private readonly Rectangle _clientRectangle;

        public GameToolTip(int width, int height, float fontSize)
        {
            _clientRectangle = new Rectangle(0, 0, width, height);
            _font = new Font("", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

            OwnerDraw = true;
            Popup += OnPopup;
            Draw += OnDraw;
        }

        private void OnPopup(object sender, PopupEventArgs e) =>
            e.ToolTipSize = _clientRectangle.Size;

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            var border = 3;
            var fillRectangle = new Rectangle(border, border, _clientRectangle.Width - (border << 1), _clientRectangle.Height - (border << 1));

            using (var brushFrame = new LinearGradientBrush(_clientRectangle, Color.Gainsboro, Color.SlateGray, 45f))
            using (var brushFill = new LinearGradientBrush(fillRectangle, Color.Navy, Color.Black, 90f))
            using (var format = new StringFormat())
            {
                format.Alignment = format.LineAlignment = StringAlignment.Center;

                e.Graphics.FillRectangle(brushFrame, _clientRectangle);
                e.Graphics.FillRectangle(brushFill, fillRectangle);
                e.Graphics.DrawString(e.ToolTipText, _font, Brushes.White, fillRectangle, format);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font.Dispose();

                Popup -= OnPopup;
                Draw -= OnDraw;
            }

            base.Dispose(disposing);
        }
    }
}
