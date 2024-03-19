using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class ChartColumnPercent
    {
        private readonly int maxHeight;
        private readonly int yDown;

        private float percent;
        private Rectangle rectangle;
        private Rectangle rectangleLabel;

        public Rectangle Rectangle => rectangle;

        public Rectangle RectangleLabel => rectangleLabel;

        public ChartColumnPercent(int x, int width, int maxHeight, int yDown)
        {
            this.maxHeight = maxHeight;
            this.yDown = yDown;

            rectangle = new Rectangle(x, yDown, width, 0);
            rectangleLabel = new Rectangle(x - width, Rectangle.Y - width, 3 * width, width);
        }

        public float Percent
        {
            set
            {
                percent = value;
                rectangle.Height = (int)(value * maxHeight / 100f);
                rectangle.Y = yDown - rectangle.Height;
                rectangleLabel.Y = Rectangle.Y - rectangleLabel.Height;
            }

            get => percent;
        }
    }
}
