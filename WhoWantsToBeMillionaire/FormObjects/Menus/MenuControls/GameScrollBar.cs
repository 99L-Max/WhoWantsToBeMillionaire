using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class GameScrollBar : PictureBox
    {
        private const int Border = 2;

        private int _value;
        private int _maximum = 100;
        private int _thumbSize = 10;
        private Rectangle _thumbRectangle;

        public readonly ScrollOrientation Orientation;
        public ScrollEventHandler Scroll;

        public int Value
        {
            set
            {
                if (_value != value)
                {
                    _value = value;

                    if (Orientation == ScrollOrientation.HorizontalScroll)
                        _thumbRectangle.X = _value * (ClientRectangle.Width - _thumbSize) / _maximum;
                    else
                        _thumbRectangle.Y = _value * (ClientRectangle.Height - _thumbSize) / _maximum;

                    OnScroll();
                    Invalidate();
                }
            }
            get => _value;
        }

        public int Maximum
        {
            set { _maximum = value; SetThumbRectangle(); Invalidate(); }
            get => _maximum;
        }

        public int ThumbSize
        {
            set { _thumbSize = value; SetThumbRectangle(); Invalidate(); }
            get => _thumbSize;
        }

        public GameScrollBar(ScrollOrientation orientation)
        {
            Orientation = orientation;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            SetThumbRectangle();
        }

        protected abstract void MouseScroll(MouseEventArgs e);

        private void SetThumbRectangle()
        {
            if (Orientation == ScrollOrientation.HorizontalScroll)
                _thumbRectangle = new Rectangle(_value * (ClientRectangle.Width - _thumbSize) / _maximum, Border, _thumbSize, ClientRectangle.Height - (Border << 1));
            else
                _thumbRectangle = new Rectangle(Border, _value * (ClientRectangle.Height - _thumbSize) / _maximum, ClientRectangle.Width - (Border << 1), _thumbSize);
        }

        protected override void OnPaint(PaintEventArgs e) =>
            e.Graphics.FillRectangle(Brushes.DarkViolet, _thumbRectangle);

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
            base.OnSizeChanged(e);
            SetThumbRectangle();
        }

        public void OnScroll(ScrollEventType type = ScrollEventType.ThumbPosition) =>
            Scroll?.Invoke(this, new ScrollEventArgs(type, Value, Orientation));
    }
}
