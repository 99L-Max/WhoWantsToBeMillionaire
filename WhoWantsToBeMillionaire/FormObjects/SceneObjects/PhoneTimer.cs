﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PhoneTimer : MovingPictureBox, IDisposable
    {
        private readonly Image background;
        private readonly Image front;
        private readonly Image ring;
        private readonly Graphics g;
        private readonly Brush brush;
        private readonly Timer timer;
        private readonly int maxSeconds;

        private int seconds;

        public delegate void EventTimeUp(object sender, SceneCommand cmd);
        public event EventTimeUp TimeUp;

        public PhoneTimer(int side) : base(side, side)
        {
            background = ResourceManager.GetImage("PhoneTimer_Back.png");
            ring = ResourceManager.GetImage("PhoneTimer_Front.png");
            front = new Bitmap(ring.Width, ring.Height);
            brush = new SolidBrush(Color.Transparent);

            g = Graphics.FromImage(front);
            g.CompositingMode = CompositingMode.SourceCopy;

            Font = new Font("", 0.35f * side, FontStyle.Bold);
            ForeColor = Color.White;
            maxSeconds = seconds = 30;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += TimerTick;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            g.DrawImage(ring, 0, 0);
            g.FillPie(brush, 0, 0, ring.Width, ring.Height, -90, (maxSeconds - seconds) * 360 / maxSeconds);

            e.Graphics.DrawImage(background, ClientRectangle);
            e.Graphics.DrawImage(front, ClientRectangle);

            TextRenderer.DrawText(e.Graphics, $"{seconds}", Font, ClientRectangle, ForeColor);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            seconds--;
            Invalidate();

            if (seconds <= 0)
            {
                timer.Stop();
                TimeUp.Invoke(this, SceneCommand.End_PhoneFriend);
            }
        }

        public void Start() 
        {
            Sound.StopAll();
            Sound.Play("Hint_PhoneFriend_Timer.wav");

            timer.Start(); 
        }

        public void Stop() => timer.Stop();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Tick -= TimerTick;

                background.Dispose();
                front.Dispose();
                ring.Dispose();
                g.Dispose();
                brush.Dispose();
                timer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}