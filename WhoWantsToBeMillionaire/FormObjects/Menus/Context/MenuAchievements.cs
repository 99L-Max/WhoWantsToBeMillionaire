using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class MenuAchievements : ContextMenu, IDisposable
    {
        private readonly TableImages table;

        public MenuAchievements(int width, int height, Dictionary<Achievement, bool> achievements) : base("Достижения", width, height, 0.05f * height)
        {
            table = new TableImages((int)(0.05f * height));
            table.Dock = DockStyle.Fill;

            using (Stream stream = ResourceManager.GetStream("Achievements.json", TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(reader.ReadToEnd());
                var sizeRow = new Size((int)(0.8f * width), (int)(0.15f * height));
                var granted = achievements.Where(x => x.Value);
                var artist = new ArtistAchievements();

                string title, comment;
                Image image = artist.GetImageProgress(granted.Count(), achievements.Count, sizeRow.Width, sizeRow.Height);

                table.Add(image);

                foreach (var ach in granted)
                {
                    (title, comment) = dict[ach.Key.ToString()];

                    using (Image icon = ResourceManager.GetImage($"Achievement_{ach.Key}.png"))
                        image = artist.GetImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                    table.Add(image);
                }

                if (granted.Count() != achievements.Count)
                {
                    var sizeText = new Size(sizeRow.Width, sizeRow.Height >> 1);
                    table.AddText("Неполученные достижения", 0.3f * sizeRow.Height, sizeText, Color.White);

                    using (Image icon = ResourceManager.GetImage($"Achievement_Locked.png"))
                        foreach (var ach in achievements.Where(x => !x.Value))
                        {
                            (title, comment) = dict[ach.Key.ToString()];
                            image = artist.GetImage(icon, title, comment, sizeRow.Width, sizeRow.Height);

                            table.Add(image);
                        }
                }
            }

            SetControls(table);
            SetHeights(6f);

            table.DrawTable();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                table.Dispose();

            base.Dispose(disposing);
        }
    }
}