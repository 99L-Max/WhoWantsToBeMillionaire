using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class Screensaver : PictureBox, IDisposable
    {
        private readonly Image background;
        private readonly Image logo;

        private Rectangle logoRectangle;
        private bool imageVisible;
        private int alpha;

        public Screensaver()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            background = new Bitmap(ResourceManager.GetImage("Background_Screensaver.png"), MainForm.ScreenRectangle.Size);
            logo = ResourceManager.GetImage("Logo.png");
            logoRectangle = new Rectangle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (imageVisible)
            {
                e.Graphics.DrawImage(background, ClientRectangle);
                e.Graphics.DrawImage(logo, logoRectangle);
            }

            if (alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        public async Task ShowSaver(bool isFullVersion)
        {
            int sizeLogo;
            imageVisible = false;

            Sound.StopAll();
            Sound.Play(isFullVersion ? "Screensaver_Full.wav" : "Screensaver_Restart.wav");

            await ShowTransition(10);

            for (float i = 0.1f; i < 0.8f; i += 0.045f)
            {
                sizeLogo = (int)(i * Height);

                logoRectangle.X = (ClientRectangle.Width - sizeLogo) >> 1;
                logoRectangle.Y = (ClientRectangle.Height - sizeLogo) >> 1;

                logoRectangle.Width = logoRectangle.Height = sizeLogo;

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

            imageVisible = !imageVisible;

            await FillRectangles(alphas.Reverse());
        }

        private async Task FillRectangles(IEnumerable<int> alphas)
        {
            foreach (var a in alphas)
            {
                alpha = a;
                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                background.Dispose();
                logo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
