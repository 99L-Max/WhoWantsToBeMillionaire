using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Painter
    {
        public Rectangle ResizeRectangle(Rectangle rectangle, float ratioWidth, float ratioHeight)
        {
            var result = rectangle;

            result.Width = (int)(ratioWidth * rectangle.Width);
            result.Height = (int)(ratioHeight * rectangle.Height);

            result.X = (rectangle.Width - result.Width >> 1) + rectangle.X;
            result.Y = (rectangle.Height - result.Height >> 1) + rectangle.Y;

            return result;
        }

        public Rectangle ResizeRectangle(Rectangle rectangle, int border)
        {
            var result = rectangle;

            result.Width = rectangle.Width - (border << 1);
            result.Height = rectangle.Height - (border << 1);

            result.X = (rectangle.Width - result.Width >> 1) + rectangle.X;
            result.Y = (rectangle.Height - result.Height >> 1) + rectangle.Y;

            return result;
        }

        public Image GetAchievementImage(Image icon, string title, string comment, int width, int height)
        {
            var image = new Bitmap(width, height);
            var sizeIcon = (int)(0.7f * height);
            var posIcon = height - sizeIcon >> 1;
            var titleRectangle = new Rectangle(height, posIcon, width - height, sizeIcon >> 1);
            var commentRectangle = titleRectangle;

            commentRectangle.Y += commentRectangle.Height;

            using (var g = Graphics.FromImage(image))
            using (var fontTitle = new Font("", 0.18f * height, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var fontComment = new Font("", 0.14f * height, GraphicsUnit.Pixel))
            using (var format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;

                g.DrawImage(icon, posIcon, posIcon, sizeIcon, sizeIcon);

                g.DrawString(title, fontTitle, Brushes.White, titleRectangle, format);
                g.DrawString(comment, fontComment, Brushes.White, commentRectangle, format);
            }

            return image;
        }

        public Image GetAchievementProgress(int countGranted, int countAchievements, int width, int height)
        {
            var image = new Bitmap(width, height);
            var textRectangle = new Rectangle(image.Height, 0, image.Width - image.Height, image.Height >> 1);
            var progressRectangle = new Rectangle(image.Height, image.Height >> 1, image.Width - image.Height, image.Height >> 1);

            progressRectangle = ResizeRectangle(progressRectangle, 0.95f, 0.3f);

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

        public Image GetFilledPanel(Size size, int border, Color colorBack1, Color colorBack2, float angleBack, Color colorFront1, Color colorFront2, float angleFront)
        {
            var image = new Bitmap(size.Width, size.Height);
            var rectFrame = new Rectangle(new Point(), size);
            var rectFill = new Rectangle(border, border, rectFrame.Width - (border << 1), rectFrame.Height - (border << 1));

            using (var g = Graphics.FromImage(image))
            using (var brushFrame = new LinearGradientBrush(rectFrame, colorBack1, colorBack2, angleBack))
            using (var brushFill = new LinearGradientBrush(rectFill, colorFront1, colorFront2, angleFront))
            {
                g.FillRectangle(brushFrame, rectFrame);
                g.FillRectangle(brushFill, rectFill);
            }

            return image;
        }
    }
}
