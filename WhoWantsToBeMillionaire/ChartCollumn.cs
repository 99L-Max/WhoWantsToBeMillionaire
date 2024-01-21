using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class ChartCollumn
    {
        private readonly int maxHeight;
        private readonly int width;
        private readonly int yMax;
        private readonly int index;

        private float percent;
        private float height;
        private float dp;

        public RectangleF LabelRectangleF { private set; get; }

        public RectangleF RectangleF { private set; get; }

        public float Percent
        {
            set 
            {
                percent = value;
                height = percent * maxHeight / 100;
                RectangleF = new RectangleF((2 * index + 1) * width, yMax + maxHeight - height, width, height);
                LabelRectangleF = new RectangleF(2 * index * width, RectangleF.Y - width, 3 * width, width);
            }
            get => percent;
        }

        public ChartCollumn(int index, int width, int maxHeight, int yMax)
        {
            this.index = index;
            this.width = width;
            this.yMax = yMax;
            this.maxHeight = maxHeight;

            dp = 7;
        }

        public void ChangePersent()
        {
            percent += dp;

            if (percent > 100 || percent < 0)
            {
                dp = -dp;
                percent += dp;
            }

            Percent = percent;
        }
    }
}
