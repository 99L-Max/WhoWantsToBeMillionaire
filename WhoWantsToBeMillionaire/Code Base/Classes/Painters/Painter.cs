using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    static class Painter
    {
        public static Image GetGradientEllipse(Size size, Color centerColor, float focusScales = 0f)
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

        public static Image Join(params Image[] images)
        {
            var width = images.Max(image => image.Width);
            var height = images.Max(image => image.Height);
            var result = new Bitmap(width, height);

            using (var g = Graphics.FromImage(result))
                foreach (var image in images)
                    g.DrawImage(image, 0, 0, width, height);

            return result;
        }
    }
}
