using System;
using System.Drawing;
using System.Threading.Tasks;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class BoxAchievement : MovingControl, IDisposable
    {
        public BoxAchievement(Achievement achievement, Size size) : base(size)
        {
            var image = new Bitmap(Width, Height);
            var dict = JsonManager.GetDictionary<Achievement, (string, string)>(Resources.Dictionary_Achievements);
            var (title, comment) = dict[achievement];

            using (var g = Graphics.FromImage(image))
            using (var icon = Painter.GetIconAchievement(achievement))
            using (var background = Painter.CreateFilledPanel(Size, 6, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f))
            using (var imageAchievement = Painter.CreateAchievementImage(icon, title, comment, Width, Height))
            {
                g.DrawImage(background, ClientRectangle);
                g.DrawImage(imageAchievement, ClientRectangle);
            }

            Image = image;
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

        public async Task ShowAchievement(int countFramesMovement, int displayTime)
        {
            await MoveX(0, countFramesMovement);
            await Task.Delay(displayTime);
            await MoveX(-Width, countFramesMovement);
        }
    }
}
