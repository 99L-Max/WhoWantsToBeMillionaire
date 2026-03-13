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
        private readonly Dictionary<LetterOptions, ChartColumnPercent> _columns;
        private readonly Image _imageColumn;
        private readonly Image _image;
        private readonly Graphics _g;

        public VotingChart(Size size) : base(size)
        {
            var keys = CollectionFactory.GetEnum<LetterOptions>();
            var widthColumn = Width / (2 * keys.Count() + 1);
            var maxHeightColumn = (int)(0.7f * Height);
            var yDown = (int)(0.8f * Height);

            BackgroundImage = Resources.AudienceChart;
            Font = FontFactory.CreateFont(GameFonts.Arial, 0.07f * Height, FontStyle.Bold);
            ForeColor = Color.White;

            _image = new Bitmap(Width, Height);
            _imageColumn = Resources.ChartColumn;
            _g = Graphics.FromImage(_image);
            _columns = keys.ToDictionary(_ => _, value => new ChartColumnPercent((2 * (int)value + 1) * widthColumn, widthColumn, maxHeightColumn, yDown));
        }

        public async Task ShowAnimationVote(int millisecond)
        {
            Sound.StopAll();
            Sound.Play(Resources.Hint_AskAudience_Voting);

            var framesCount = millisecond / GameConst.DeltaTime;
            var deltaPercent = _columns.Keys.ToDictionary(key => key, _ => GameRandom.NextFloat() * 7f + 3f);

            do
            {
                foreach (var column in _columns)
                {
                    column.Value.Percent += deltaPercent[column.Key];

                    if (column.Value.Percent < GameConst.MinPercent || column.Value.Percent > GameConst.MaxPercent)
                    {
                        deltaPercent[column.Key] = -deltaPercent[column.Key];
                        column.Value.Percent += deltaPercent[column.Key];
                    }
                }

                DrawChart(false);
                await Task.Delay(GameConst.DeltaTime);
            }
            while (--framesCount > 0);
        }

        public async Task ShowPercents(Dictionary<LetterOptions, int> percents, int framesCount)
        {
            Sound.StopAll();
            Sound.Play(Resources.Hint_AskAudience_End);

            foreach (var column in _columns.Values)
                column.Percent = 0;

            var deltaPercent = percents.ToDictionary(pair => pair.Key, pair => (float)pair.Value / framesCount);

            do
            {
                foreach (var key in percents.Keys)
                    _columns[key].Percent += deltaPercent[key];

                DrawChart(false);
                await Task.Delay(GameConst.DeltaTime);
            }
            while (--framesCount > 0);

            foreach (var percent in percents)
                _columns[percent.Key].Percent = percent.Value;

            DrawChart(true);
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

            foreach (var column in _columns.Values)
                _g.DrawImage(_imageColumn, column.Rectangle);

            if (labelsVisible)
                foreach (var column in _columns.Values)
                    TextRenderer.DrawText(_g, $"{column.Percent:f0}%", Font, column.LabelRectangle, ForeColor);

            Image = _image;
        }
    }
}