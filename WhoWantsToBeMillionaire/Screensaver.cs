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
        private readonly Bitmap background;
        private readonly Bitmap logo;

        private bool imageVisible;
        private float sideLogo;
        private int alpha;

        public Screensaver()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            background = new Bitmap(ResourceProcessing.GetImage("Background_Screensaver.png"), MainForm.RectScreen.Size);
            logo = new Bitmap(ResourceProcessing.GetImage("Logo.png"));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (imageVisible)
            {
                e.Graphics.DrawImage(background, ClientRectangle);
                e.Graphics.DrawImage(logo, (ClientRectangle.Width - sideLogo) / 2f, (ClientRectangle.Height - sideLogo) / 2f, sideLogo, sideLogo);
            }

            if (alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        public new async Task Show()
        {
            imageVisible = false;
            sideLogo = 0f;

            await ShowTransition(5);

            for (float i = 0.1f; i < 0.8f; i += 0.045f)
            {
                sideLogo = i * Height;
                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            }

            await Task.Delay(3000);

            await ShowTransition(10);

            Visible = false;
        }

        private async Task ShowTransition(int countFrames)
        {
            List<int> alphas = Enumerable.Range(0, countFrames).Select(a => byte.MaxValue * a / (countFrames - 1)).ToList();

            await FillRectangles(alphas);

            imageVisible = !imageVisible;
            alphas.Reverse();

            await FillRectangles(alphas);
        }

        private async Task FillRectangles(List<int> alphas)
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
                background?.Dispose();
                logo?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
