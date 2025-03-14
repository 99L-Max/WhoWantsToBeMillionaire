﻿using System;
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

        private bool _imageVisible;
        private int _alpha;
        private Rectangle _logoRectangle;

        public Screensaver()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            _background = new Bitmap(Resources.Background_Screensaver, GameConst.ScreenSize);
            _logoRectangle = new Rectangle();
            _logo = Resources.Logo;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_imageVisible)
            {
                e.Graphics.DrawImage(_background, ClientRectangle);
                e.Graphics.DrawImage(_logo, _logoRectangle);
            }

            if (_alpha > 0)
                using (var brush = new SolidBrush(Color.FromArgb(_alpha, Color.White)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
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
                await Task.Delay(GameConst.DeltaTime);
            }
        }

        public async Task ShowSaver(bool isFullVersion)
        {
            int sizeLogo;
            _imageVisible = false;

            GameSound.Play(isFullVersion ? Resources.Screensaver_Full : Resources.Screensaver_Restart);

            await ShowTransition(10);

            for (float i = 0.1f; i < 0.8f; i += 0.045f)
            {
                sizeLogo = (int)(i * Height);

                _logoRectangle.X = ClientRectangle.Width - sizeLogo >> 1;
                _logoRectangle.Y = ClientRectangle.Height - sizeLogo >> 1;

                _logoRectangle.Width = _logoRectangle.Height = sizeLogo;

                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }

            await Task.Delay(isFullVersion ? 7000 : 2000);

            await ShowTransition(20);
        }
    }
}
