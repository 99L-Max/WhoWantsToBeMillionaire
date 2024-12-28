using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameToolTip : ToolTip, IDisposable
    {
        private readonly Rectangle _clientRectangle;
        private readonly Rectangle _textRectangle;
        private readonly Image _background;
        private readonly Font _font;

        public GameToolTip(int width, int height, int border, float fontSize)
        {
            _font = FontManager.CreateFont(GameFont.Arial, fontSize, FontStyle.Bold);
            _clientRectangle = new Rectangle(0, 0, width, height);
            _textRectangle = Resizer.ResizeRectangle(_clientRectangle, border);
            _background = Painter.CreateFilledPanel(_clientRectangle.Size, border, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);

            OwnerDraw = true;
            Popup += OnPopup;
            Draw += OnDraw;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font.Dispose();
                _background.Dispose();

                Popup -= OnPopup;
                Draw -= OnDraw;
            }

            base.Dispose(disposing);
        }

        private void OnPopup(object sender, PopupEventArgs e) =>
            e.ToolTipSize = _clientRectangle.Size;

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            using (var format = new StringFormat())
            {
                format.Alignment = format.LineAlignment = StringAlignment.Center;

                e.Graphics.DrawImage(_background, _clientRectangle);
                e.Graphics.DrawString(e.ToolTipText, _font, Brushes.White, _textRectangle, format);
            }
        }
    }
}
