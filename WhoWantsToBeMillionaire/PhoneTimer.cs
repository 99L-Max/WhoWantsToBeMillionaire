using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PhoneTimer : MovingPictureBox, IDisposable
    {
        private readonly Bitmap background;

        private int seconds;
        private bool isRun;

        public int Seconds
        { 
            private set
            {
                seconds = value;
                Invalidate();
            } 
            get => seconds; 
        }

        public delegate void EventTimeUp();
        public event EventTimeUp TimeUp;

        public PhoneTimer(int side) : base(new Size(side, side))
        {
            background = new Bitmap(ResourceProcessing.GetImage("PhoneTimer.png"));
            Font = new Font("", 0.4f * side, FontStyle.Bold);
            ForeColor = Color.White;
            isRun = false;
            seconds = 30;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(background, ClientRectangle);
            TextRenderer.DrawText(e.Graphics, $"{seconds}", Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        public async void Start()
        {
            isRun = true;

            while (Seconds > 0 && isRun)
            {
                await Task.Delay(1000);
                Seconds--;
            }

            TimeUp.Invoke();
        }

        public void Stop()
        {
            isRun = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                background.Dispose();

            base.Dispose(disposing);
        }
    }
}
