using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    class BoxAchievement : MovingPictureBox, IDisposable
    {
        public BoxAchievement(Achievement achievement, int width, int height) : base(width, height)
        {
            var image = new Bitmap(width, height);

            using (Image icon = ResourceManager.GetImage($"Achievement_{achievement}.png"))
            using (Graphics g = Graphics.FromImage(image))
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(64, 64, 64), Color.FromArgb(32, 32, 32), 90f))
            using (Stream stream = ResourceManager.GetStream("Achievements.json", TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(reader.ReadToEnd());
                var (title, comment) = dict[$"{achievement}"];
                var artist = new ArtistAchievements();

                g.FillRectangle(brush, ClientRectangle);

                using (Image art = artist.GetImage(icon, title, comment, width, height))
                    g.DrawImage(art, ClientRectangle);

                Image = image;
            }
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
