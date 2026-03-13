using System;
using System.Drawing;
using System.Threading.Tasks;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class BoxAchievement : MovingControl, IDisposable
    {
        public BoxAchievement(Achievements achievement, Size size) : base(size)
        {
            var image = new Bitmap(Width, Height);
            var title = JsonReader.GetDictionary<Achievements, AchievementText>(Resources.Dictionary_Achievements)[achievement].Title;

            using (var g = Graphics.FromImage(image))
            using (var icon = AchievementPainter.GetAchievementIcon(achievement, true))
            using (var imageAchievement = AchievementPainter.GetAchievementImage(icon, Size, "Получено достижение!", title, Color.Magenta, Color.White, 0.25f, 0.25f))
                g.DrawImage(imageAchievement, ClientRectangle);

            BackgroundImage = PanelPainter.GetLinearGradientPanel(Size, 6, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);
            Image = image;
        }

        public async Task ShowAchievement(int countFramesMovement, int displayTime)
        {
            Sound.Play(Resources.Achievement, false);

            await MoveX(0, countFramesMovement);
            await Task.Delay(displayTime);
            await MoveX(-Width, countFramesMovement);
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
