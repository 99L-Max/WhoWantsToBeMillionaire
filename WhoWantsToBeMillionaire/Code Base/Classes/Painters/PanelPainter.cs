using System.Drawing;
using System.Drawing.Drawing2D;

namespace WhoWantsToBeMillionaire
{
    static class PanelPainter
    {
        public static Image GetLinearGradientPanel(Size size, int border, Color colorBack1, Color colorBack2, float angleBack, Color colorFront1, Color colorFront2, float angleFront)
        {
            using (var panel = GetLinearGradientPanel(size, colorFront1, colorFront2, angleFront))
                return GetLinearGradientPanel(size, border, colorBack1, colorBack2, angleBack, panel);
        }

        public static Image GetLinearGradientPanel(Size size, int border, Color colorBack1, Color colorBack2, float angleBack, Image frontImage)
        {
            var result = GetLinearGradientPanel(size, colorBack1, colorBack2, angleBack);
            var rectangle = Resizer.ResizeRectangle(new Rectangle(new Point(), size), border);

            using (var g = Graphics.FromImage(result))
                g.DrawImage(frontImage, rectangle);

            return result;
        }

        public static Image GetLinearGradientPanel(Size size, Color color1, Color color2, float angle)
        {
            var image = new Bitmap(size.Width, size.Height);
            var rectangle = new Rectangle(new Point(), size);

            using (var g = Graphics.FromImage(image))
            using (var brush = new LinearGradientBrush(rectangle, color1, color2, angle))
            {
                g.FillRectangle(brush, rectangle);
            }

            return image;
        }
    }
}
