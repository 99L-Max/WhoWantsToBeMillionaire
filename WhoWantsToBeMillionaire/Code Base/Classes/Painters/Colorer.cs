using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace WhoWantsToBeMillionaire
{
    static class Colorer
    {
        public static void Recolor(Image image, Color targetColor)
        {
            var max = Convert.ToSingle(byte.MaxValue);
            var r = targetColor.R / max;
            var g = targetColor.G / max;
            var b = targetColor.B / max;
            var a = targetColor.A / max;

            var colorMatrix = new ColorMatrix(new float[][]
            {
                 new float[] {r, 0, 0, 0, 0},
                 new float[] {0, g, 0, 0, 0},
                 new float[] {0, 0, b, 0, 0},
                 new float[] {0, 0, 0, a, 0},
                 new float[] {0, 0, 0, 0, 1}
            });

            var rectange = new Rectangle(0, 0, image.Width, image.Height);

            using (var attributes = new ImageAttributes())
            using (var graphics = Graphics.FromImage(image))
            {
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, rectange, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }
}
