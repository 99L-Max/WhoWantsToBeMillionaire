using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class PhoneTimer : MovingControl, IDisposable
    {
        private readonly List<Image> _framesBackground;
        private readonly List<Image> _framesForeground;
        private readonly Timer _timerSeconds = new Timer();
        private readonly Timer _timerFrames = new Timer();

        private int _indexFrame = 0;
        private int _seconds;

        public Action<object, SceneCommand> TimeUp;

        public PhoneTimer(int side, int seconds) : base(side, side)
        {
            _framesBackground = Painter.CutSprite(Resources.PhoneTimer_Background, 5, 5);
            _framesForeground = Painter.CutSprite(Resources.PhoneTimer_Foreground, 5, 5);

            _seconds = seconds;

            Font = FontManager.CreateFont(GameFont.Copperplate, 0.42f * side, FontStyle.Bold);
            ForeColor = Color.Silver;

            _timerSeconds.Interval = 1000;
            _timerFrames.Interval = 40;

            _timerSeconds.Tick += OnTimerSecondsTick;
            _timerFrames.Tick += OnTimerFramesTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerSeconds.Tick -= OnTimerSecondsTick;
                _timerFrames.Tick -= OnTimerFramesTick;

                _timerSeconds.Dispose();
                _timerFrames.Dispose();

                _framesBackground.ForEach(f => f.Dispose());
                _framesForeground.ForEach(f => f.Dispose());

                Font.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_framesBackground[_indexFrame], ClientRectangle);

            TextRenderer.DrawText(e.Graphics, $"{_seconds}", Font, ClientRectangle, ForeColor);

            e.Graphics.DrawImage(_framesForeground[_indexFrame], ClientRectangle);
        }

        private void OnTimerFramesTick(object sender, EventArgs e)
        {
            if (++_indexFrame >= _framesBackground.Count)
                _indexFrame = 0;

            Invalidate();
        }

        private void OnTimerSecondsTick(object sender, EventArgs e)
        {
            if (--_seconds <= 0)
            {
                _timerSeconds.Stop();
                TimeUp?.Invoke(this, SceneCommand.End_PhoneFriend);
            }
        }

        public void Start()
        {
            Sound.StopAll();
            Sound.Play(Resources.Hint_PhoneFriend_Timer);

            _timerSeconds.Start();
            _timerFrames.Start();
        }

        public void Stop()
        {
            _timerSeconds.Stop();
            _timerFrames.Stop();
        }
    }
}