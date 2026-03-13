using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuAchievements : ContextMenu
    {
        private readonly TableImages _table;

        public MenuAchievements(float fractionScreenHeight, int widthFraction, int heightFraction, Dictionary<Achievements, bool> achievements) :
            base("Достижения", fractionScreenHeight, widthFraction, heightFraction)
        {
            var achievementsTexts = JsonReader.GetDictionary<Achievements, AchievementText>(Resources.Dictionary_Achievements);
            var rowSize = new Size((int)(0.8f * Width), (int)(0.15f * Height));
            var granted = achievements.Where(achievement => achievement.Value);
            var image = AchievementPainter.GetAchievementProgress(granted.Count(), achievements.Count, rowSize.Width, rowSize.Height);

            _table = new TableImages(10);
            _table.Add(image);

            if (granted.Count() > 0)
            {
                using (var background = AchievementPainter.GetAchievementIconBackground(true))
                {
                    foreach (var key in granted.Select(pair => pair.Key))
                    {
                        achievementsTexts[key].GetData(out string title, out string comment);

                        using (var icon = AchievementPainter.GetAchievementIcon(key, true))
                        using (var fillIcon = Painter.Join(background, icon))
                        {
                            image = AchievementPainter.GetAchievementImage(fillIcon, rowSize, title, comment, Color.White, Color.White);
                        }

                        _table.Add(image);
                    }
                }
            }

            if (granted.Count() != achievements.Count)
            {
                var textSize = new Size(rowSize.Width, rowSize.Height >> 1);
                var ungranted = achievements.Where(achievement => achievement.Value == false);

                _table.AddText("Неполученные достижения", 0.3f * rowSize.Height, textSize, Color.White);

                using (var background = AchievementPainter.GetAchievementIconBackground(false))
                using (var icon = AchievementPainter.GetAchievementIcon(ungranted.First().Key, false))
                using (var fillIcon = Painter.Join(background, icon))
                {
                    foreach (var key in ungranted.Select(pair => pair.Key))
                    {
                        achievementsTexts[key].GetData(out string title, out string comment);
                        image = AchievementPainter.GetAchievementImage(fillIcon, rowSize, title, comment, Color.White, Color.White);
                        _table.Add(image);
                    }
                }
            }

            SetControls(_table);
            SetHeights(6);

            _table.DrawTable();
        }
    }
}