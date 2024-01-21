using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class AudienceChart : MovingPictureBox, IDisposable
    {
        private readonly Random random;
        private readonly Bitmap background;
        private readonly Bitmap image;
        private readonly Bitmap imageColumn;
        private readonly Graphics g;
        private readonly Question question;
        private readonly Dictionary<char, ChartCollumn> columns;

        public AudienceChart(Size size, Question question) : base(size)
        {
            random = new Random();

            background = new Bitmap(ResourceProcessing.GetImage("AudienceChart.png"));
            imageColumn = new Bitmap(ResourceProcessing.GetImage("ChartColumn.png"));
            image = new Bitmap(background.Width, background.Height);
            g = Graphics.FromImage(image);

            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;
            Font = new Font("", 0.05f * background.Height);

            BackgroundImage = background;

            this.question = question;

            columns = new Dictionary<char, ChartCollumn>();
            char[] keys = question.Options.Keys.ToArray();
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

        public async Task ShowResult(int countFrames)
        {
            List<char> keys = question.Options.Where(x => x.Key != question.Correct && x.Value != string.Empty).Select(x => x.Key).OrderBy(k => random.Next()).ToList();
            List<float> persents = new List<float>();
            int sum = 101, randomPersent;

            for (int i = 0; i < keys.Count; i++)
            {
                randomPersent = random.Next(sum);
                persents.Add(randomPersent);
                sum -= randomPersent;
            }

            persents.Add(sum);
            persents = persents.OrderByDescending(x => x).ToList();

            if (random.NextDouble() <= -0.05 * question.Number + 1.25 || keys.Count == 2)
                keys.Insert(0, question.Correct);
            else
                keys.Insert(random.Next(keys.Count) + 1, question.Correct);

            foreach (var c in columns.Values)
                c.Percent = 0;
            DrawChart(false);

            Dictionary<char, float> dp = new Dictionary<char, float>();
            for (int i = 0; i < keys.Count; i++)
                dp.Add(keys[i], persents[i] / countFrames);

            do
            {
                foreach (var key in keys)
                    columns[key].Percent += dp[key];
                DrawChart(false);

                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);


            for (int i = 0; i < keys.Count; i++)
                columns[keys[i]].Percent = persents[i];
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
