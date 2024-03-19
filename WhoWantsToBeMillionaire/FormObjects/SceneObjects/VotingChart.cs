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
        private readonly Image imageColumn;
        private readonly Dictionary<Letter, ChartColumnPercent> columns;

        private bool labelsVisible = false;

        public VotingChart(int width, int height) : base(width, height)
        {
            var keys = Question.Letters;
            var widthColumn = width / (2 * keys.Count() + 1);
            var maxHeightColumn = (int)(0.7f * height);
            var yDown = (int)(0.8f * height);

            BackgroundImage = ResourceManager.GetImage("AudienceChart.png");
            Font = new Font("", 0.05f * height, FontStyle.Bold);
            imageColumn = ResourceManager.GetImage("ChartColumn.png");
            columns = keys.ToDictionary(k => k, v => new ChartColumnPercent((2 * (int)v + 1) * widthColumn, widthColumn, maxHeightColumn, yDown));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(BackgroundImage, ClientRectangle);

            foreach (var c in columns.Values)
                e.Graphics.DrawImage(imageColumn, c.Rectangle);

            if (labelsVisible)
                foreach (var c in columns.Values)
                    TextRenderer.DrawText(e.Graphics, $"{c.Percent:f0}%", Font, c.RectangleLabel, Color.White);
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            Sound.PlayBackground("Hint_AskAudience_Voting.wav");

            labelsVisible = false;

            var countFrames = millisecond / MainForm.DeltaTime;
            var random = new Random();
            var dp = columns.Keys.ToDictionary(k => k, v => (float)random.NextDouble() * 7f + 3f);

            do
            {
                foreach (var col in columns)
                {
                    col.Value.Percent += dp[col.Key];

                    if (col.Value.Percent < 0 || col.Value.Percent > 100)
                    {
                        dp[col.Key] = -dp[col.Key];
                        col.Value.Percent += dp[col.Key];
                    }
                }

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);
        }

        public async Task ShowPercents(Dictionary<Letter, int> percents, int countFrames)
        {
            Sound.StopAll();
            Sound.Play("Hint_AskAudience_End.wav");

            foreach (var col in columns.Values)
                col.Percent = 0;

            var dp = percents.ToDictionary(k => k.Key, v => (float)v.Value / countFrames);

            do
            {
                foreach (var key in percents.Keys)
                    columns[key].Percent += dp[key];

                Invalidate();
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            foreach (var p in percents)
                columns[p.Key].Percent = p.Value;

            labelsVisible = true;

            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackgroundImage.Dispose();
                imageColumn.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
