using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class BoxAchievement : MovingPictureBox, IDisposable
    {
        public BoxAchievement(Achievement achievement, int width, int height) : base(width, height)
        {
            var image = new Bitmap(width, height);
            var sizeIcon = (int)(0.7f * height);
            var posIcon = (height - sizeIcon) >> 1;
            var textRectangle = new Rectangle(height, posIcon, width - height, sizeIcon >> 1);

            using (Graphics g = Graphics.FromImage(image))
            using (Image icon = ResourceManager.GetImage($"Achievement_{achievement}.png"))
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(64, 64, 64), Color.FromArgb(32, 32, 32), 90f))
            using (Font fontTitle = new Font("", 0.2f * height, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font fontComment = new Font("", 0.18f * height, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Stream stream = ResourceManager.GetStream("Achievements.json", TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(reader.ReadToEnd());
                var (title, comment) = dict[$"{achievement}"];

                g.FillRectangle(brush, ClientRectangle);
                g.DrawImage(icon, posIcon, posIcon, sizeIcon, sizeIcon);

                TextRenderer.DrawText(g, title, fontTitle, textRectangle, Color.White, TextFormatFlags.Left);

                textRectangle.Y += textRectangle.Height;

                TextRenderer.DrawText(g, comment, fontComment, textRectangle, Color.White, TextFormatFlags.Left);

                Image = image;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Image.Dispose();

            base.Dispose(disposing);
        }
    }
}
