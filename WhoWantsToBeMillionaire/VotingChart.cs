using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WhoWantsToBeMillionaire
{
    class VotingChart : MovingPictureBox, IDisposable
    {
        private readonly Bitmap background;
        private readonly Bitmap image;
        private readonly Bitmap imageColumn;
        private readonly Graphics g;
        private readonly Dictionary<Letter, ChartCollumn> columns;

        public VotingChart(Size size) : base(size)
        {
            background = new Bitmap(ResourceProcessing.GetImage("AudienceChart.png"));
            imageColumn = new Bitmap(ResourceProcessing.GetImage("ChartColumn.png"));
            image = new Bitmap(background.Width, background.Height);
            g = Graphics.FromImage(image);

            Font = new Font("", 0.05f * background.Height);

            BackgroundImage = background;

            columns = new Dictionary<Letter, ChartCollumn>();
            Letter[] keys = Enum.GetValues(typeof(Letter)).Cast<Letter>().ToArray();
            int widthColumn = background.Width / (2 * keys.Length + 1);

            for (int i = 0; i < keys.Length; i++)
                columns.Add(keys[i], new ChartCollumn(i, widthColumn, 210, 45));
        }

        private void DrawChart(bool labelsVisible)
        {
            g.Clear(Color.Transparent);

            foreach (var c in columns.Values)
                g.DrawImage(imageColumn, c.RectangleF);

            if (labelsVisible)
                using (Brush brush = new SolidBrush(Color.White))
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    foreach (var c in columns.Values)
                        g.DrawString($"{c.Percent}%", Font, brush, c.LabelRectangleF, format);
                }

            Image = image;
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            Random random = new Random();

            foreach (var c in columns.Values)
                c.Percent = random.Next(101);

            int countFrames = millisecond / MainForm.DeltaTime;

            do
            {
                foreach (var c in columns.Values)
                    c.ChangePersent();

                DrawChart(false);
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);
        }

        public async Task ShowPercents(int countFrames, Dictionary<Letter, float> percents)
        {
            foreach (var c in columns.Values)
                c.Percent = 0;
            DrawChart(false);

            Dictionary<Letter, float> dp = percents.ToDictionary(k => k.Key, v => v.Value / countFrames);

            do
            {
                foreach (var key in percents.Keys)
                    columns[key].Percent += dp[key];
                DrawChart(false);

                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            foreach (var key in percents.Keys)
                columns[key].Percent = percents[key];
            DrawChart(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackgroundImage = Image = null;
                g.Dispose();
                background.Dispose();
                image.Dispose();
                imageColumn.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
