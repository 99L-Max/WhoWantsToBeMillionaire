using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class RowTableSums : PictureBox
    {
        private static readonly Image background;
        private static readonly Image iconCircle;
        private static readonly Image iconRhomb;
        private static readonly Size sizeNumber;
        private static readonly Size sizeSum;
        private static readonly TextFormatFlags textFormatFlags;

        private readonly Image image;

        private bool isSelected;
        private bool iconVisible;
        private bool isSaveSum;
        private bool isMouseEventsActive;

        public readonly int Number;
        public readonly int Sum;

        public bool IsSelected
        {
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    BackgroundImage = isSelected ? background : null;

                    if (iconVisible)
                        Draw();
                }
            }
            get => isSelected;
        }

        public bool IconVisible
        {
            set
            {
                if (iconVisible != value)
                {
                    iconVisible = value;
                    Draw();
                }
            }
            get => iconVisible;
        }

        public bool IsSaveSum
        {
            set
            {
                if (isSaveSum != value)
                {
                    isSaveSum = value;
                    Draw();
                }
            }
            get => isSaveSum;
        }

        static RowTableSums()
        {
            int height = 60;

            background = new Bitmap(ResourceManager.GetImage("CurrentAmount.png"), 500, height);
            iconCircle = new Bitmap(ResourceManager.GetImage("IconSum_Circle.png"), height, height);
            iconRhomb = new Bitmap(ResourceManager.GetImage("IconSum_Rhomb.png"), height, height);

            sizeNumber = new Size((int)(0.18f * background.Width), background.Height);
            sizeSum = new Size((int)(0.90f * background.Width), background.Height);
            textFormatFlags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
        }

        public RowTableSums(int number, int sum)
        {
            Number = number;
            Sum = sum;

            image = new Bitmap(background.Width, background.Height);
            isMouseEventsActive = false;

            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;
            Font = new Font("", 0.4f * background.Height, FontStyle.Bold);
            Dock = DockStyle.Fill;

            Reset();
        }

        private void Draw()
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);

                Color[] colors = { Color.Black, isSaveSum ? Color.White : Color.Orange };
                Point[] points = { new Point(2, 2), new Point() };

                for (int i = 0; i < points.Length; i++)
                {
                    TextRenderer.DrawText(g, $"{Number}", Font, new Rectangle(points[i], sizeNumber), colors[i], textFormatFlags);
                    TextRenderer.DrawText(g, String.Format("{0:#,0}", Sum), Font, new Rectangle(points[i], sizeSum), colors[i], textFormatFlags);
                }

                if (iconVisible)
                    g.DrawImage(isSelected ? iconCircle : iconRhomb, sizeNumber.Width, 0, background.Height, background.Height);

                Image = image;
            }
        }

        public void Reset()
        {
            iconVisible = isSaveSum = isSelected = false;
            BackgroundImage = null;

            RemoveMouseEvents();
            Draw();
        }

        public void AddMouseEvents()
        {
            if (!isMouseEventsActive)
            {
                isMouseEventsActive = true;
                MouseEnter += OnRowMouseEnter;
                MouseLeave += OnRowMouseLeave;
            }
        }

        public void RemoveMouseEvents()
        {
            if (isMouseEventsActive)
            {
                isMouseEventsActive = false;
                MouseEnter -= OnRowMouseEnter;
                MouseLeave -= OnRowMouseLeave;
            }
        }

        private void OnRowMouseLeave(object sender, EventArgs e) => IsSelected = false;

        private void OnRowMouseEnter(object sender, EventArgs e) => IsSelected = true;
    }
}