using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class ChartColumnPercent
    {
        private readonly int _maxHeight;
        private readonly int _yDown;

        private float _percent;
        private Rectangle _rectangle;
        private Rectangle _labelRectangle;

        public Rectangle Rectangle => _rectangle;

        public Rectangle RectangleLabel => _labelRectangle;

        public ChartColumnPercent(int x, int width, int maxHeight, int yDown)
        {
            _maxHeight = maxHeight;
            _yDown = yDown;

            _rectangle = new Rectangle(x, yDown, width, 0);
            _labelRectangle = new Rectangle(x - width, Rectangle.Y - width, 3 * width, width);
        }

        public float Percent
        {
            set
            {
                _percent = value;
                _rectangle.Height = (int)(value * _maxHeight / 100f);
                _rectangle.Y = _yDown - _rectangle.Height;
                _labelRectangle.Y = Rectangle.Y - _labelRectangle.Height;
            }

            get => _percent;
        }
    }
}
