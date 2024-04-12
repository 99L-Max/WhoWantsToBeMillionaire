using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class PhoneTimer : MovingControl, IDisposable
    {
        private readonly Image _image;
        private readonly Image _ring;
        private readonly Graphics _g;
        private readonly Brush _brush;
        private readonly Timer _timer;
        private readonly int _maxSeconds;

        private int _seconds;

        public Action<object, SceneCommand> TimeUp;

        public PhoneTimer(int side) : base(side, side)
        {
            Font = new Font("", 0.45f * side, FontStyle.Bold, GraphicsUnit.Pixel);
            ForeColor = Color.White;
            BackgroundImage = Resources.PhoneTimer_Back;

            _ring = Resources.PhoneTimer_Front;
            _image = new Bitmap(_ring.Width, _ring.Height);
            _brush = new SolidBrush(Color.Transparent);
            _timer = new Timer();
            _g = Graphics.FromImage(_image);

            _g.CompositingMode = CompositingMode.SourceCopy;

            _timer.Interval = 1000;
            _timer.Tick += OnTimerTick;

            SetSeconds(_maxSeconds = 30);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Tick -= OnTimerTick;

                _image.Dispose();
                _ring.Dispose();
                _g.Dispose();
                _brush.Dispose();
                _timer.Dispose();

                BackgroundImage.Dispose();
                Font.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            e.Graphics.DrawImage(_image, ClientRectangle);

            TextRenderer.DrawText(e.Graphics, $"{_seconds}", Font, ClientRectangle, ForeColor);
        }

        private void SetSeconds(int value)
        {
            _seconds = value;

            _g.DrawImage(_ring, 0, 0);
            _g.FillPie(_brush, 0, 0, _ring.Width, _ring.Height, -90, (_maxSeconds - _seconds) * 360 / _maxSeconds);

            Invalidate();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            SetSeconds(_seconds - 1);

            if (_seconds <= 0)
            {
                _timer.Stop();
                TimeUp?.Invoke(this, SceneCommand.End_PhoneFriend);
            }
        }

        public void Start()
        {
            Sound.StopAll();
            Sound.Play(Resources.Hint_PhoneFriend_Timer);

            _timer.Start();
        }

        public void Stop() =>
            _timer.Stop();
    }
}