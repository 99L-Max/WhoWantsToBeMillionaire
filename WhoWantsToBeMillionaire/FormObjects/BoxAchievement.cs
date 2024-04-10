using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class BoxAchievement : MovingControl, IDisposable
    {
        public BoxAchievement(Achievement achievement, int width, int height) : base(width, height)
        {
            var image = new Bitmap(width, height);

            using (var icon = (Image)Resources.ResourceManager.GetObject($"Achievement_{achievement}"))
            using (var g = Graphics.FromImage(image))
            using (var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(64, 64, 64), Color.FromArgb(32, 32, 32), 90f))
            {
                var dict = JsonManager.GetDictionary<Achievement, (string, string)>(Resources.Dictionary_Achievements);
                var (title, comment) = dict[achievement];
                var artist = new Painter();

                g.FillRectangle(brush, ClientRectangle);

                using (var art = artist.GetAchievementImage(icon, title, comment, width, height))
                    g.DrawImage(art, ClientRectangle);

                Image = image;
            }
        }

        public async Task ShowAchievement(int y, int countFrames, int delay)
        {
            await MoveY(y, countFrames);
            await Task.Delay(delay);
            await MoveX(-Width, countFrames);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image?.Dispose();
                BackgroundImage?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
