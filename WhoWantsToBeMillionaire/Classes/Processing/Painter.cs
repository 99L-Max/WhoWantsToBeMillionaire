﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class Painter
    {
        public static Image GetIconAchievement(Achievement achievement)
        {
            var spriteSize = new Size(3, 5);
            var index = (int)(achievement);
            var result = Resources.Achievement_Background;

            using (var sprite = Resources.Achievement_Icons)
            using (var icon = CutSprite(sprite, spriteSize.Width, spriteSize.Height, index / spriteSize.Height, index % spriteSize.Height))
            using (var g = Graphics.FromImage(result))
                g.DrawImage(icon, 0, 0, result.Width, result.Height);

            return result;
        }

        public static Image CreateAchievementImage(Image icon, string title, string comment, int width, int height)
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

        public static Image CreateAchievementProgress(int countGranted, int countAchievements, int width, int height)
        {
            var image = new Bitmap(width, height);
            var textRectangle = new Rectangle(image.Height, 0, image.Width - image.Height, image.Height >> 1);
            var progressRectangle = new Rectangle(image.Height, image.Height >> 1, image.Width - image.Height, image.Height >> 1);

            progressRectangle = Resizer.ResizeRectangle(progressRectangle, 0.95f, 0.3f);

            using (var g = Graphics.FromImage(image))
            using (var sprite = Resources.Medal)
            using (var medal = CutSprite(sprite, 1, 2, 0, Convert.ToInt32(countGranted == countAchievements)))
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

        public static Image CutSprite(Image sprite, int rowsCount, int columnsCount, int row, int column)
        {
            var width = sprite.Width / columnsCount;
            var height = sprite.Height / rowsCount;

            var cropArea = new Rectangle(column * width, row * height, width, height);
            var croppedImage = new Bitmap(cropArea.Width, cropArea.Height);

            using (var g = Graphics.FromImage(croppedImage))
                g.DrawImage(sprite, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height), cropArea, GraphicsUnit.Pixel);

            return croppedImage;
        }
    }
}