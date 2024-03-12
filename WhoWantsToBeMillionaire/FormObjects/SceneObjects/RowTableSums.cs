using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class RowTableSums : PictureBox
    {
        private static readonly Bitmap background;
        private static readonly Bitmap iconCircle;
        private static readonly Bitmap iconRhomb;
        private static readonly StringFormat stringFormat;

        private readonly Bitmap image;

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

            stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };
        }

        public RowTableSums(int number, int sum)
        {
            Number = number;
            Sum = sum;

            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;
            Font = new Font("", 0.4f * background.Height, FontStyle.Bold);
            Dock = DockStyle.Fill;

            image = new Bitmap(background.Width, background.Height);

            isMouseEventsActive = false;

            Reset();
        }

        private void Draw()
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                Size sizeNum = new Size((int)(0.18f * background.Width), background.Height);
                Size sizeSum = new Size((int)(0.90f * background.Width), background.Height);

                Brush[] brushes = {
                    new SolidBrush(Color.Black),
                    new SolidBrush(isSaveSum ? Color.White : Color.Orange)
                };

                Point[] points = { new Point(2, 2), new Point() };

                g.Clear(Color.Transparent);

                for (int i = 0; i < points.Length; i++)
                {
                    g.DrawString($"{Number}", Font, brushes[i], new RectangleF(points[i], sizeNum), stringFormat);
                    g.DrawString(String.Format("{0:#,0}", Sum), Font, brushes[i], new RectangleF(points[i], sizeSum), stringFormat);
                    brushes[i].Dispose();
                }

                if (iconVisible)
                    g.DrawImage(isSelected ? iconCircle : iconRhomb, sizeNum.Width, 0);

                Image = image;
            }
        }

        public void Reset()
        {
            iconVisible = false;
            isSaveSum = false;
            isSelected = false;

            BackgroundImage = null;

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