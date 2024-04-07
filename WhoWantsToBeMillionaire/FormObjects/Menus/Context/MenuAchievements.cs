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
            _table = new TableImages((int)(0.05f * height));
            _table.Dock = DockStyle.Fill;

            var dict = JsonManager.GetDictionary<string, (string, string)>(Resources.Dictionary_Achievements);
            var sizeRow = new Size((int)(0.8f * width), (int)(0.15f * height));
            var granted = achievements.Where(x => x.Value);
            var artist = new ArtistAchievements();
            var image = artist.GetImageProgress(granted.Count(), achievements.Count, sizeRow.Width, sizeRow.Height);

            string title, comment;

            _table.Add(image);

            foreach (var ach in granted)
            {
                (title, comment) = dict[ach.Key.ToString()];

                using (Image icon = (Image)Resources.ResourceManager.GetObject($"Achievement_{ach.Key}"))
                    image = artist.GetImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                _table.Add(image);
            }

            if (granted.Count() != achievements.Count)
            {
                var sizeText = new Size(sizeRow.Width, sizeRow.Height >> 1);
                _table.AddText("Неполученные достижения", 0.3f * sizeRow.Height, sizeText, Color.White);

                using (Image icon = Resources.Achievement_Locked)
                    foreach (var ach in achievements.Where(x => !x.Value))
                    {
                        (title, comment) = dict[ach.Key.ToString()];
                        image = artist.GetImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                        _table.Add(image);
                    }
            }

            SetControls(_table);
            SetHeights(6f);

            _table.DrawTable();
        }
    }
}