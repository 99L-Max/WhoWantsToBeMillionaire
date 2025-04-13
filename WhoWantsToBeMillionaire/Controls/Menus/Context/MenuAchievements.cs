using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuAchievements : ContextMenu
    {
        private readonly TableImages _table;

        public MenuAchievements(float fractionScreenHeight, int widthFraction, int heightFraction, Dictionary<Achievement, bool> achievements) :
            base("Достижения", fractionScreenHeight, widthFraction, heightFraction)
        {
            var dict = JsonManager.GetDictionary<Achievement, (string, string)>(Resources.Dictionary_Achievements);
            var sizeRow = new Size((int)(0.8f * Width), (int)(0.15f * Height));
            var granted = achievements.Where(x => x.Value);
            var image = Painter.CreateAchievementProgress(granted.Count(), achievements.Count, sizeRow.Width, sizeRow.Height);

            string title, comment;

            _table = new TableImages(10);
            _table.Add(image);

            foreach (var achievement in granted)
            {
                (title, comment) = dict[achievement.Key];

                using (var icon = Painter.GetIconAchievement(achievement.Key, true))
                    image = Painter.CreateAchievementImage(icon, sizeRow, title, comment, Color.White, Color.White);

                _table.Add(image);
            }

            if (granted.Count() != achievements.Count)
            {
                var sizeText = new Size(sizeRow.Width, sizeRow.Height >> 1);
                _table.AddText("Неполученные достижения", 0.3f * sizeRow.Height, sizeText, Color.White);

                using (var icon = Resources.Achievement_Locked)
                    foreach (var achievement in achievements.Where(x => !x.Value))
                    {
                        (title, comment) = dict[achievement.Key];

                        image = Painter.CreateAchievementImage(icon, sizeRow, title, comment, Color.White, Color.White);

                        _table.Add(image);
                    }
            }

            SetControls(_table);
            SetHeights(6);

            _table.DrawTable();
        }
    }
}