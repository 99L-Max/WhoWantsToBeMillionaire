using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class ChartCollumn
    {
        private readonly int maxHeight;
        private readonly int yMax;

        private Rectangle rectangle;
        private Rectangle textRectangle;
        private float percent;
        private float dPercent;

        public Rectangle Rectangle => rectangle;

        public Rectangle TextRectangle => textRectangle;

        public float Percent
        {
            set
            {
                percent = value;

                rectangle.Height = (int)(percent * maxHeight / 100f);

                rectangle.Y = yMax + maxHeight - rectangle.Height;
                textRectangle.Y = Rectangle.Y - textRectangle.Height;
            }
            get => percent;
        }

        public ChartCollumn(int index, int width, int maxHeight, int yMax)
        {
            this.yMax = yMax;
            this.maxHeight = maxHeight;

            rectangle = new Rectangle((2 * index + 1) * width, yMax + maxHeight, width, 0);
            textRectangle = new Rectangle(2 * index * width, Rectangle.Y - width, 3 * width, width);
        }

        public void SetChangePerсent(float dp) => dPercent = dp;

        public void ChangePerсent()
        {
            percent += dPercent;

            if (percent > 100 || percent < 0)
            {
                dPercent = -dPercent;
                percent += dPercent;
            }

            Percent = percent;
        }
    }
}
