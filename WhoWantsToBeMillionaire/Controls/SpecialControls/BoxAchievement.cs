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
            var comment = dict[achievement].Item1;

            using (var g = Graphics.FromImage(image))
            using (var icon = Painter.GetIconAchievement(achievement, false))
            using (var background = Painter.CreateFilledPanel(Size, 6, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f))
            using (var imageAchievement = Painter.CreateAchievementImage(icon, Size, "Получено достижение!", comment, Color.Magenta, Color.White, 0.25f, 0.25f))
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
            GameSound.Play(Resources.Achievement, false);

            await MoveX(0, countFramesMovement);
            await Task.Delay(displayTime);
            await MoveX(-Width, countFramesMovement);
        }
    }
}
