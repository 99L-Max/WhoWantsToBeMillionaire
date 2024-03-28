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
        public BoxAchievement(int width, int height) : base(width, height)
        {
            SetBackground();
        }

        public BoxAchievement(Achievement achievement, int width, int height) : base(width, height)
        {
            using (Image icon = ResourceManager.GetImage($"Achievement_{achievement}.png"))
            using (Stream stream = ResourceManager.GetStream("Achievements.json", TypeResource.Dictionaries))
            using (StreamReader reader = new StreamReader(stream))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(reader.ReadToEnd());
                var (title, comment) = dict[$"{achievement}"];

                SetBackground();
                SetImage(icon, title, comment, width, height);
            }
        }

        public BoxAchievement(Image icon, string title, string comment, int width, int height) : base(width, height)
        {
            SetBackground();
            SetImage(icon, title, comment, width, height);
        }

        private void SetBackground()
        {
            Image background = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using (Graphics g = Graphics.FromImage(background))
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(64, 64, 64), Color.FromArgb(32, 32, 32), 90f))
            {
                g.FillRectangle(brush, ClientRectangle);

                BackgroundImage?.Dispose();
                BackgroundImage = background;
            }
        }

        protected void SetImage(Image icon, string title, string comment, int width, int height)
        {
            var image = new Bitmap(width, height);
            var sizeIcon = (int)(0.7f * height);
            var posIcon = (height - sizeIcon) >> 1;
            var titleRectangle = new Rectangle(height, posIcon, width - height, sizeIcon >> 1);
            var commentRectangle = titleRectangle;

            commentRectangle.Y += commentRectangle.Height;

            using (Graphics g = Graphics.FromImage(image))
            using (Font fontTitle = new Font("", 0.2f * height, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font fontComment = new Font("", 0.18f * height, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                g.DrawImage(icon, posIcon, posIcon, sizeIcon, sizeIcon);

                TextRenderer.DrawText(g, title, fontTitle, titleRectangle, Color.White, TextFormatFlags.Left);
                TextRenderer.DrawText(g, comment, fontComment, commentRectangle, Color.White, TextFormatFlags.Left);

                Image?.Dispose();
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
