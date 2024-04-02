using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class VotingChart : MovingPictureBox, IDisposable
    {
        private readonly Dictionary<Letter, ChartColumnPercent> _columns;
        private readonly Image _imageColumn;
        private readonly Image _image;
        private readonly Graphics _g;

        private bool labelsVisible = false;

        public VotingChart(int width, int height) : base(width, height)
        {
            var keys = Question.Letters;
            var widthColumn = width / (2 * keys.Count() + 1);
            var maxHeightColumn = (int)(0.7f * height);
            var yDown = (int)(0.8f * height);

            BackgroundImage = Resources.AudienceChart;
            Font = new Font("", 0.07f * height, FontStyle.Bold, GraphicsUnit.Pixel);
            ForeColor = Color.White;

            _imageColumn = Resources.ChartColumn;
            _image = new Bitmap(width, height);
            _g = Graphics.FromImage(_image);
            _columns = keys.ToDictionary(k => k, v => new ChartColumnPercent((2 * (int)v + 1) * widthColumn, widthColumn, maxHeightColumn, yDown));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            e.Graphics.DrawImage(_image, ClientRectangle);
        }

        private void DrawChart()
        {
            _g.Clear(Color.Transparent);

            foreach (var c in _columns.Values)
                _g.DrawImage(_imageColumn, c.Rectangle);

            if (labelsVisible)
                foreach (var c in _columns.Values)
                    TextRenderer.DrawText(_g, $"{c.Percent:f0}%", Font, c.RectangleLabel, ForeColor);

            Invalidate();
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            Sound.PlayBackground(Resources.Hint_AskAudience_Voting);

            labelsVisible = false;

            var countFrames = millisecond / MainForm.DeltaTime;
            var random = new Random();
            var dp = _columns.Keys.ToDictionary(k => k, v => (float)random.NextDouble() * 7f + 3f);

            do
            {
                foreach (var col in _columns)
                {
                    col.Value.Percent += dp[col.Key];

                    if (col.Value.Percent < 0 || col.Value.Percent > 100)
                    {
                        dp[col.Key] = -dp[col.Key];
                        col.Value.Percent += dp[col.Key];
                    }
                }

                DrawChart();
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);
        }

        public async Task ShowPercents(Dictionary<Letter, int> percents, int countFrames)
        {
            Sound.StopAll();
            Sound.Play(Resources.Hint_AskAudience_End);

            foreach (var col in _columns.Values)
                col.Percent = 0;

            var dp = percents.ToDictionary(k => k.Key, v => (float)v.Value / countFrames);

            do
            {
                foreach (var key in percents.Keys)
                    _columns[key].Percent += dp[key];

                DrawChart();
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            foreach (var p in percents)
                _columns[p.Key].Percent = p.Value;

            labelsVisible = true;

            DrawChart();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _g.Dispose();
                _image.Dispose();
                _imageColumn.Dispose();

                BackgroundImage.Dispose();
                Font.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
