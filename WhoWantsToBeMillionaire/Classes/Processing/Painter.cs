using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class Painter
    {
        public static Image CutSprite(Image sprite, int rowsCount, int columnsCount, int row, int column, bool isDisposeSprite = true)
        {
            var width = sprite.Width / columnsCount;
            var height = sprite.Height / rowsCount;

            var destRect = new Rectangle(0, 0, width, height);
            var srcRect = new Rectangle(column * width, row * height, width, height);

            var result = new Bitmap(destRect.Width, destRect.Height);

            using (var g = Graphics.FromImage(result))
                g.DrawImage(sprite, destRect, srcRect, GraphicsUnit.Pixel);

            if (isDisposeSprite)
                sprite.Dispose();

            return result;
        }

        public static List<Image> CutSprite(Image sprite, int rowsCount, int columnsCount, bool isDisposeSprite = true)
        {
            var list = new List<Image>();

            for (int row = 0; row < rowsCount; row++)
                for (int column = 0; column < columnsCount; column++)
                    list.Add(CutSprite(sprite, rowsCount, columnsCount, row, column, false));

            if (isDisposeSprite)
                sprite.Dispose();

            return list;
        }

        public static ReadOnlyDictionary<TKey, Image> GetThemeImages<TKey>(Image sprite)
        {
            var keys = Enum.GetValues(typeof(TKey)).Cast<TKey>();
            var images = CutSprite(sprite, keys.Count(), 1);
            var dict = keys.Zip(images, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            return new ReadOnlyDictionary<TKey, Image>(dict);
        }

        public static Image GetIconAchievement(Achievement achievement, bool isAddBackground)
        {
            var spriteSize = new Size(3, 5);
            var index = (int)achievement;
            var icon = CutSprite(Resources.Achievement_Icons, spriteSize.Width, spriteSize.Height, index / spriteSize.Height, index % spriteSize.Height);

            if (!isAddBackground)
                return icon;

            var background = CreateFilledPanel(icon.Size, 3, Color.Gainsboro, Color.SlateGray, 45f, Color.Indigo, Color.Black, 90f);
            var g = Graphics.FromImage(background);

            g.DrawImage(icon, 0, 0, background.Width, background.Height);

            g.Dispose();
            icon.Dispose();

            return background;
        }

        public static Image CreateAchievementImage(Image icon, Size size, string title, string comment, Color colorTitle, Color colorComment, float ratioFontTitle = 0.25f, float ratioFontComment = 0.18f)
        {
            var image = new Bitmap(size.Width, size.Height);
            var sizeIcon = (int)(0.7f * size.Height);
            var posIcon = size.Height - sizeIcon >> 1;
            var titleRectangle = new Rectangle(size.Height, posIcon, size.Width - size.Height, sizeIcon >> 1);
            var commentRectangle = titleRectangle;

            commentRectangle.Y += commentRectangle.Height;

            using (var g = Graphics.FromImage(image))
            using (var font = FontManager.CreateFont(GameFont.Arial, ratioFontTitle * size.Height, FontStyle.Bold))
            using (var fontComment = FontManager.CreateFont(GameFont.Arial, ratioFontComment * size.Height))
            {

                g.DrawImage(icon, posIcon, posIcon, sizeIcon, sizeIcon);

                TextRenderer.DrawText(g, title, font, titleRectangle, colorTitle, TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, comment, fontComment, commentRectangle, colorComment, TextFormatFlags.VerticalCenter);
            }

            return image;
        }

        public static Image CreateAchievementProgress(int countGranted, int countAchievements, int width, int height)
        {
            var image = new Bitmap(width, height);
            var textRectangle = new Rectangle(image.Height, 0, image.Width - image.Height, image.Height >> 1);
            var progressRectangle = new Rectangle(image.Height, image.Height >> 1, image.Width - image.Height, image.Height >> 1);

            progressRectangle = Resizer.ResizeRectangle(progressRectangle, 0.95f, 0.3f);

            using (var g = Graphics.FromImage(image))
            using (var medal = CutSprite(Resources.Medal, 1, 2, 0, Convert.ToInt32(countGranted == countAchievements)))
            using (var font = FontManager.CreateFont(GameFont.Arial, 0.2f * height, FontStyle.Bold))
            {
                g.DrawImage(medal, 0, 0, image.Height, image.Height);

                TextRenderer.DrawText(g, $"Получено {countGranted} из {countAchievements} достижений ({100 * countGranted / countAchievements}%)", font, textRectangle, Color.White);

                g.FillRectangle(Brushes.Black, progressRectangle);

                progressRectangle.Width = progressRectangle.Width * countGranted / countAchievements;

                g.FillRectangle(Brushes.DodgerBlue, progressRectangle);
            }

            return image;
        }

        public static Image CreateFilledPanel(Size size, int border, Color colorBack1, Color colorBack2, float angleBack, Color colorFront1, Color colorFront2, float angleFront)
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

        public static Image CreateGradientEllipse(Size size, Color centerColor, float focusScales = 0f)
        {
            var ellipse = new Bitmap(size.Width, size.Height);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, size.Width, size.Height);

                using (var pathGradientBrush = new PathGradientBrush(path))
                using (var g = Graphics.FromImage(ellipse))
                {
                    pathGradientBrush.CenterColor = centerColor;
                    pathGradientBrush.SurroundColors = new Color[] { Color.Transparent };
                    pathGradientBrush.FocusScales = new PointF(focusScales, focusScales);

                    g.FillEllipse(pathGradientBrush, 0, 0, size.Width, size.Height);
                }
            }

            return ellipse;
        }
    }
}
