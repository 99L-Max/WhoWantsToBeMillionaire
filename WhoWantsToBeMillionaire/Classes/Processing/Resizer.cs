﻿using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    static class Resizer
    {
        public static Image ResizeImage(Image image, float ratio) =>
            ResizeImage(image, ratio, ratio);

        public static Image ResizeImage(Image image, float ratioWidth, float ratioHeight)
        {
            var result = new Bitmap(image.Width, image.Height);
            var rectangle = new Rectangle(new Point(), image.Size);

            using (var g = Graphics.FromImage(result))
                g.DrawImage(image, ResizeRectangle(rectangle, ratioWidth, ratioHeight));

            return result;
        }

        public static Rectangle ResizeRectangle(Rectangle rectangle, float ratio) =>
            ResizeRectangle(rectangle, ratio, ratio);

        public static Rectangle ResizeRectangle(Rectangle rectangle, float ratioWidth, float ratioHeight)
        {
            var result = rectangle;

            result.Width = (int)(ratioWidth * rectangle.Width);
            result.Height = (int)(ratioHeight * rectangle.Height);

            result.X = (rectangle.Width - result.Width >> 1) + rectangle.X;
            result.Y = (rectangle.Height - result.Height >> 1) + rectangle.Y;

            return result;
        }

        public static Rectangle ResizeRectangle(Rectangle rectangle, int border)
        {
            var result = rectangle;

            result.Width = rectangle.Width - (border << 1);
            result.Height = rectangle.Height - (border << 1);

            result.X = (rectangle.Width - result.Width >> 1) + rectangle.X;
            result.Y = (rectangle.Height - result.Height >> 1) + rectangle.Y;

            return result;
        }
    }
}