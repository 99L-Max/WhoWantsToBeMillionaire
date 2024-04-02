using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    public class GameScrollBar : PictureBox
    {
        private const int Border = 2;

        private int _value;
        private int _maximum = 100;
        private int _thumbSize = 10;
        private ScrollOrientation _orientation;

        public event ScrollEventHandler Scroll;

        public ScrollOrientation Orientation
        {
            set
            {
                _orientation = value;

                var (max, min) = ClientRectangle.Width > ClientRectangle.Height ? (ClientRectangle.Width, ClientRectangle.Height) : (ClientRectangle.Height, ClientRectangle.Width);

                Size = value == ScrollOrientation.HorizontalScroll ? new Size(max, min) : new Size(min, max);
            }
            get => _orientation;
        }

        public int Value
        {
            set
            {
                if (_value != value)
                {
                    _value = value;

                    OnScroll();
                    Invalidate();
                }
            }
            get => _value;
        }

        public int Maximum
        {
            set { _maximum = value; Invalidate(); }
            get => _maximum;
        }

        public int ThumbSize
        {
            set { _thumbSize = value; Invalidate(); }
            get => _thumbSize;
        }

        public GameScrollBar(ScrollOrientation orientation)
        {
            BackColor = Color.FromArgb(128, Color.Black);
            Size = new Size(15, 100);
            Orientation = orientation;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseScroll(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseScroll(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OnScroll(ScrollEventType.EndScroll);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_orientation == ScrollOrientation.HorizontalScroll)
                ThumbSize = (int)(0.2f * ClientRectangle.Width);
            else
                ThumbSize = (int)(0.2f * ClientRectangle.Height);
        }

        private void MouseScroll(MouseEventArgs e)
        {
            int value;

            if (Orientation == ScrollOrientation.VerticalScroll)
                value = _maximum * (e.Y - (_thumbSize >> 1)) / (ClientRectangle.Height - _thumbSize);
            else
                value = _maximum * (e.X - (_thumbSize >> 1)) / (ClientRectangle.Width - _thumbSize);

            Value = Math.Max(0, Math.Min(_maximum, value));
        }

        public virtual void OnScroll(ScrollEventType type = ScrollEventType.ThumbPosition) =>
            Scroll?.Invoke(this, new ScrollEventArgs(type, Value, Orientation));

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_maximum <= 0)
                return;

            Rectangle thumbRect;

            if (Orientation == ScrollOrientation.HorizontalScroll)
                thumbRect = new Rectangle(_value * (ClientRectangle.Width - _thumbSize) / _maximum, Border, _thumbSize, ClientRectangle.Height - (Border << 1));
            else
                thumbRect = new Rectangle(Border, _value * (ClientRectangle.Height - _thumbSize) / _maximum, ClientRectangle.Width - (Border << 1), _thumbSize);

            e.Graphics.FillRectangle(Brushes.DarkViolet, thumbRect);
        }
    }
}
