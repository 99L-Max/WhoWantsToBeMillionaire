using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Screensaver : PictureBox, IDisposable
    {
        private readonly Image _background;
        private readonly Image _logo;

        private Rectangle _logoRectangle;
        private bool _imageVisible;
        private int _alpha;

        public Screensaver()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            _background = new Bitmap(Resources.Background_Screensaver, MainForm.ScreenSize);
            _logo = Resources.Logo;
            _logoRectangle = new Rectangle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_imageVisible)
            {
                e.Graphics.DrawImage(_background, ClientRectangle);
                e.Graphics.DrawImage(_logo, _logoRectangle);
            }

            if (_alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(_alpha, Color.White)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        public async Task ShowSaver(bool isFullVersion)
        {
            int sizeLogo;
            _imageVisible = false;

            Sound.StopAll();
            Sound.Play(isFullVersion ? Resources.Screensaver_Full : Resources.Screensaver_Restart);

            await ShowTransition(10);

            for (float i = 0.1f; i < 0.8f; i += 0.045f)
            {
                sizeLogo = (int)(i * Height);

                _logoRectangle.X = (ClientRectangle.Width - sizeLogo) >> 1;
                _logoRectangle.Y = (ClientRectangle.Height - sizeLogo) >> 1;

                _logoRectangle.Width = _logoRectangle.Height = sizeLogo;

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await Task.Delay(isFullVersion ? 7000 : 2000);

            await ShowTransition(20);
        }

        private async Task ShowTransition(int countFrames)
        {
            var alphas = Enumerable.Range(0, countFrames).Select(a => byte.MaxValue * a / (countFrames - 1));

            await FillRectangles(alphas);

            _imageVisible = !_imageVisible;

            await FillRectangles(alphas.Reverse());
        }

        private async Task FillRectangles(IEnumerable<int> alphas)
        {
            foreach (var a in alphas)
            {
                _alpha = a;
                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _background.Dispose();
                _logo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
