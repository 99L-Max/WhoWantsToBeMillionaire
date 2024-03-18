using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class VotingChart : MovingPictureBox, IDisposable
    {
        private readonly Image image;
        private readonly Image imageColumn;
        private readonly Graphics g;
        private readonly Dictionary<Letter, ChartCollumn> columns;

        public VotingChart(int width, int height) : base(width, height)
        {
            BackgroundImage = ResourceManager.GetImage("AudienceChart.png");
            Font = new Font("", 0.05f * BackgroundImage.Height);

            imageColumn = ResourceManager.GetImage("ChartColumn.png");
            image = new Bitmap(BackgroundImage.Width, BackgroundImage.Height);
            g = Graphics.FromImage(image);

            Letter[] keys = Enum.GetValues(typeof(Letter)).Cast<Letter>().ToArray();
            int widthColumn = BackgroundImage.Width / (2 * keys.Length + 1);
            columns = keys.ToDictionary(k => k, v => new ChartCollumn((int)v, widthColumn, 210, 45));
        }

        private void DrawChart(bool percentsVisible)
        {
            g.Clear(Color.Transparent);

            foreach (var c in columns.Values)
                g.DrawImage(imageColumn, c.Rectangle);

            if (percentsVisible)
                foreach (var c in columns.Values)
                    TextRenderer.DrawText(g, $"{c.Percent}%", Font, c.TextRectangle, Color.White);

            Image = image;
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            Sound.PlayBackground("Hint_AskAudience_Voting.wav");

            int countFrames = millisecond / MainForm.DeltaTime;
            Random random = new Random();

            foreach (var c in columns.Values)
            {
                c.Percent = 0;
                c.SetChangePerсent(random.Next(3, 8));
            }

            do
            {
                foreach (var c in columns.Values)
                    c.ChangePerсent();

                DrawChart(false);
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);
        }

        public async Task ShowPercents(int countFrames, Dictionary<Letter, float> percents)
        {
            Sound.StopAll();
            Sound.Play("Hint_AskAudience_End.wav");

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
                BackgroundImage.Dispose();

                g.Dispose();
                image.Dispose();
                imageColumn.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
