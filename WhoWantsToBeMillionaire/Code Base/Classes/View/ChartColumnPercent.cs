using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class ChartColumnPercent
    {
        private readonly int _maxHeight;
        private readonly int _yDown;

        private float _percent;

        public ChartColumnPercent(int x, int width, int maxHeight, int yDown)
        {
            _maxHeight = maxHeight;
            _yDown = yDown;

            Rectangle = new Rectangle(x, yDown, width, 0);
            LabelRectangle = new Rectangle(x - width, Rectangle.Y - width, 3 * width, width);
        }

        public Rectangle Rectangle { get; private set; }

        public Rectangle LabelRectangle { get; private set; }

        public float Percent
        {
            get => _percent;
            set => SetPercent(value);
        }

        private void SetPercent(float percent)
        {
            var rectangle = Rectangle;
            var labelRectangle = LabelRectangle;

            rectangle.Height = (int)(percent * _maxHeight / GameConst.MaxPercent);
            rectangle.Y = _yDown - rectangle.Height;
            labelRectangle.Y = rectangle.Y - labelRectangle.Height;

            _percent = percent;

            Rectangle = rectangle;
            LabelRectangle = labelRectangle;
        }
    }
}
