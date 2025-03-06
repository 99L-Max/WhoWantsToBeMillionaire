using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class VotingChart : MovingControl, IDisposable
    {
        private readonly Dictionary<LetterOption, ChartColumnPercent> _columns;
        private readonly Image _imageColumn;
        private readonly Image _image;
        private readonly Graphics _g;

        public VotingChart(Size size) : base(size)
        {
            var keys = Question.LettersOption;
            var widthColumn = Width / (2 * keys.Count() + 1);
            var maxHeightColumn = (int)(0.7f * Height);
            var yDown = (int)(0.8f * Height);

            BackgroundImage = Resources.AudienceChart;
            Font = FontManager.CreateFont(GameFont.Arial, 0.07f * Height, FontStyle.Bold);
            ForeColor = Color.White;

            _image = new Bitmap(Width, Height);
            _imageColumn = Resources.ChartColumn;
            _g = Graphics.FromImage(_image);
            _columns = keys.ToDictionary(k => k, v => new ChartColumnPercent((2 * (int)v + 1) * widthColumn, widthColumn, maxHeightColumn, yDown));
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

        private void DrawChart(bool labelsVisible)
        {
            _g.Clear(Color.Transparent);

            foreach (var c in _columns.Values)
                _g.DrawImage(_imageColumn, c.Rectangle);

            if (labelsVisible)
                foreach (var c in _columns.Values)
                    TextRenderer.DrawText(_g, $"{c.Percent:f0}%", Font, c.RectangleLabel, ForeColor);

            Image = _image;
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            GameSound.StopAll();
            GameSound.Play(Resources.Hint_AskAudience_Voting);

            var countFrames = millisecond / GameConst.DeltaTime;
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

                DrawChart(false);
                await Task.Delay(GameConst.DeltaTime);
            } while (--countFrames > 0);
        }

        public async Task ShowPercents(Dictionary<LetterOption, int> percents, int countFrames)
        {
            GameSound.StopAll();
            GameSound.Play(Resources.Hint_AskAudience_End);

            foreach (var col in _columns.Values)
                col.Percent = 0;

            var dp = percents.ToDictionary(k => k.Key, v => (float)v.Value / countFrames);

            do
            {
                foreach (var key in percents.Keys)
                    _columns[key].Percent += dp[key];

                DrawChart(false);
                await Task.Delay(GameConst.DeltaTime);
            } while (--countFrames > 0);

            foreach (var p in percents)
                _columns[p.Key].Percent = p.Value;

            DrawChart(true);
        }
    }
}