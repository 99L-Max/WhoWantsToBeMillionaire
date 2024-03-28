using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class AchievementProgressBar : BoxAchievement
    {
        public AchievementProgressBar(int width, int height, int value, int maxValue) : base(width, height)
        {
            using (Image medal = ResourceManager.GetImage($"Medal_{(value == maxValue ? "Granted" : "Empty")}.png"))
            {
                var maxProgress = 25;
                var progress = maxProgress * value / maxValue;
                var comment = $"{new string('■', progress)}{new string('□', maxProgress - progress)} ({value * 100 / maxValue}%)";
                
                SetImage(medal, $"Получено {value} из {maxValue} достижений", comment, width, height);
            }
        }
    }
}
