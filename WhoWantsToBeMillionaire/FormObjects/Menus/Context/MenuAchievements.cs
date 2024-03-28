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
        private readonly TableLayoutPanel table;

        public MenuAchievements(int width, int height, Dictionary<Achievement, bool> achievements) : base("Достижения", width, height, 0.04f * height)
        {
            table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.AutoScroll = true;

            using (Stream stream = ResourceManager.GetStream("Achievements.json", TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(reader.ReadToEnd());
                var sizeBox = new Size((int)(0.8f * width), (int)(0.15f * height));
                var granted = achievements.Where(x => x.Value);
                var allGranted = granted.Count() == achievements.Count;
                var progress = new AchievementProgressBar(sizeBox.Width, sizeBox.Height, granted.Count(), achievements.Count);

                BoxAchievement box;
                string title, comment;

                AddToTable(progress, sizeBox.Height);

                foreach (var ach in granted)
                {
                    (title, comment) = dict[ach.Key.ToString()];

                    using (Image icon = ResourceManager.GetImage($"Achievement_{ach.Key}.png"))
                        box = new BoxAchievement(icon, title, comment, sizeBox.Width, sizeBox.Height);

                    AddToTable(box, sizeBox.Height);
                }

                if (!allGranted)
                {
                    var label = new LabelMenu(0.2f * sizeBox.Height, ContentAlignment.MiddleCenter);
                    label.Text = "Неполученные достижения";

                    AddToTable(label, sizeBox.Height >> 1);

                    using (Image icon = ResourceManager.GetImage($"Achievement_Locked.png"))
                        foreach (var ach in achievements.Where(x => !x.Value))
                        {
                            (title, comment) = dict[ach.Key.ToString()];
                            box = new BoxAchievement(icon, title, comment, sizeBox.Width, sizeBox.Height);

                            AddToTable(box, sizeBox.Height);
                        }
                }
            }

            SetControls(table);
            SetHeights(6f);
        }

        private void AddToTable(Control control, int height)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            table.Controls.Add(control);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var ctrl in table.Controls)
                    (ctrl as IDisposable).Dispose();

                table.Controls.Clear();
                table.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
