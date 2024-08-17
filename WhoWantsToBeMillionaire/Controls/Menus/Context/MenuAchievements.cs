using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuAchievements : ContextMenu
    {
        private readonly TableImages _table;

        public MenuAchievements(int width, int height, Dictionary<Achievement, bool> achievements) : base("Достижения", width, height, 0.05f * height)
        {
            var dict = JsonManager.GetDictionary<string, (string, string)>(Resources.Dictionary_Achievements);
            var sizeRow = new Size((int)(0.8f * width), (int)(0.15f * height));
            var granted = achievements.Where(x => x.Value);
            var image = Painter.CreateAchievementProgress(granted.Count(), achievements.Count, sizeRow.Width, sizeRow.Height);

            string title, comment;

            _table = new TableImages((int)(0.05f * height));
            _table.Dock = DockStyle.Fill;
            _table.Add(image);

            foreach (var achievement in granted)
            {
                (title, comment) = dict[achievement.Key.ToString()];

                using (var icon = Painter.GetIconAchievement(achievement.Key))
                    image = Painter.CreateAchievementImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                _table.Add(image);
            }

            if (granted.Count() != achievements.Count)
            {
                var sizeText = new Size(sizeRow.Width, sizeRow.Height >> 1);
                _table.AddText("Неполученные достижения", 0.3f * sizeRow.Height, sizeText, Color.White);

                using (var icon = Resources.Achievement_Locked)
                    foreach (var achievement in achievements.Where(x => !x.Value))
                    {
                        (title, comment) = dict[achievement.Key.ToString()];
                        image = Painter.CreateAchievementImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                        _table.Add(image);
                    }
            }

            SetControls(_table);
            SetHeights(6f);

            _table.DrawTable();
        }
    }
}