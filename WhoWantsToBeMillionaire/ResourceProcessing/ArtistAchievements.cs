using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ArtistAchievements
    {
        public Image GetImage(Image icon, string title, string comment, int width, int height)
        {
            var image = new Bitmap(width, height);
            var sizeIcon = (int)(0.7f * height);
            var posIcon = (height - sizeIcon) >> 1;
            var titleRectangle = new Rectangle(height, posIcon, width - height, sizeIcon >> 1);
            var commentRectangle = titleRectangle;
            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

            commentRectangle.Y += commentRectangle.Height;

            using (var g = Graphics.FromImage(image))
            using (var fontTitle = new Font("", 0.16f * height, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var fontComment = new Font("", 0.16f * height, GraphicsUnit.Pixel))
            {
                g.DrawImage(icon, posIcon, posIcon, sizeIcon, sizeIcon);

                TextRenderer.DrawText(g, title, fontTitle, titleRectangle, Color.White, flags);
                TextRenderer.DrawText(g, comment, fontComment, commentRectangle, Color.White, flags);
            }

            return image;
        }

        public Image GetImageProgress(int countGranted, int countAchievements, int width, int height)
        {
            var image = new Bitmap(width, height);
            var textRectangle = new Rectangle(image.Height, 0, image.Width - image.Height, image.Height >> 1);
            var progressRectangle = new Rectangle(image.Height, image.Height >> 1, image.Width - image.Height, image.Height >> 1);

            progressRectangle = RatioRectangle(progressRectangle, 0.95f, 0.3f);

            using (var g = Graphics.FromImage(image))
            using (var medal = countGranted == countAchievements ? Resources.Medal_Granted : Resources.Medal_Empty)
            using (var font = new Font("", 0.2f * height, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                g.DrawImage(medal, 0, 0, image.Height, image.Height);

                TextRenderer.DrawText(g, $"Получено {countGranted} из {countAchievements} достижений ({100 * countGranted / countAchievements}%)", font, textRectangle, Color.White);

                g.FillRectangle(Brushes.Black, progressRectangle);

                progressRectangle.Width = progressRectangle.Width * countGranted / countAchievements;

                g.FillRectangle(Brushes.DodgerBlue, progressRectangle);
            }

            return image;
        }

        private Rectangle RatioRectangle(Rectangle rectangle, float ratioWidth, float ratioHeight)
        {
            var result = rectangle;

            result.Width = (int)(ratioWidth * rectangle.Width);
            result.Height = (int)(ratioHeight * rectangle.Height);

            result.X = (rectangle.Width - result.Width >> 1) + rectangle.X;
            result.Y = (rectangle.Height - result.Height >> 1) + rectangle.Y;

            return result;
        }
    }
}
