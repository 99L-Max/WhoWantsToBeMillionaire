using System;
using System.Drawing;
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
            {
                var dict = JsonManager.GetDictionary<Achievement, (string, string)>(Resources.Dictionary_Achievements);
                var (title, comment) = dict[achievement];
                var painter = new Painter();

                using (var background = painter.GetFilledPanel(Size, 6, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f))
                using (var imageAchievement = painter.GetAchievementImage(icon, title, comment, width, height))
                {
                    g.DrawImage(background, ClientRectangle);
                    g.DrawImage(imageAchievement, ClientRectangle);
                }

                Image = image;
            }
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

        public async Task ShowAchievement(int y, int countFrames, int delay)
        {
            await MoveY(y, countFrames);
            await Task.Delay(delay);
            await MoveX(-Width, countFrames);
        }
    }
}
