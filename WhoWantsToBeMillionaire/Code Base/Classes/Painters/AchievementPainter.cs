using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class AchievementPainter
    {
        public static Image GetAchievementIconBackground(bool isGranted)
        {
            using (var frontImage = Resources.Achievement_Background)
            {
                Colorer.Recolor(frontImage, isGranted ? Color.Fuchsia : Color.Gray);
                return PanelPainter.GetLinearGradientPanel(frontImage.Size, 3, Color.Gainsboro, Color.SlateGray, 45f, frontImage);
            }
        }

        public static Image GetAchievementIcon(Achievements achievement, bool isGranted)
        {
            if (isGranted)
            {
                var spriteSize = new Size(3, 5);
                var index = Convert.ToInt32(achievement);

                return SpritePainter.GetSprite(Resources.Achievement_Icons, spriteSize.Width, spriteSize.Height, index / spriteSize.Height, index % spriteSize.Height);
            }
            else
            {
                return Resources.Achievement_Icon_Locked;
            }
        }

        public static Image GetAchievementImage(Image icon, Size size, string title, string comment, Color colorTitle, Color colorComment, float ratioFontTitle = 0.25f, float ratioFontComment = 0.18f)
        {
            var image = new Bitmap(size.Width, size.Height);
            var iconSize = (int)(0.7f * size.Height);
            var iconLocation = size.Height - iconSize >> 1;
            var titleRectangle = new Rectangle(size.Height, iconLocation, size.Width - size.Height, iconSize >> 1);
            var commentRectangle = titleRectangle;

            commentRectangle.Y += commentRectangle.Height;

            using (var g = Graphics.FromImage(image))
            using (var font = FontFactory.CreateFont(GameFonts.Arial, ratioFontTitle * size.Height, FontStyle.Bold))
            using (var fontComment = FontFactory.CreateFont(GameFonts.Arial, ratioFontComment * size.Height))
            {
                g.DrawImage(icon, iconLocation, iconLocation, iconSize, iconSize);

                TextRenderer.DrawText(g, title, font, titleRectangle, colorTitle, TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, comment, fontComment, commentRectangle, colorComment, TextFormatFlags.VerticalCenter);
            }

            return image;
        }

        public static Image GetAchievementProgress(int grantedCount, int totalCount, int width, int height)
        {
            var image = new Bitmap(width, height);
            var textRectangle = new Rectangle(image.Height, 0, image.Width - image.Height, image.Height >> 1);
            var progressRectangle = new Rectangle(image.Height, image.Height >> 1, image.Width - image.Height, image.Height >> 1);

            progressRectangle = Resizer.ResizeRectangle(progressRectangle, 0.95f, 0.3f);

            using (var g = Graphics.FromImage(image))
            using (var medal = SpritePainter.GetSprite(Resources.Medal, 1, 2, 0, Convert.ToInt32(grantedCount == totalCount)))
            using (var font = FontFactory.CreateFont(GameFonts.Arial, 0.2f * height, FontStyle.Bold))
            {
                g.DrawImage(medal, 0, 0, image.Height, image.Height);

                TextRenderer.DrawText(g, $"Получено {grantedCount} из {totalCount} достижений ({GameConst.MaxPercent * grantedCount / totalCount}%)", font, textRectangle, Color.White);

                g.FillRectangle(Brushes.Black, progressRectangle);

                progressRectangle.Width = progressRectangle.Width * grantedCount / totalCount;

                g.FillRectangle(Brushes.DodgerBlue, progressRectangle);
            }

            return image;
        }
    }
}
