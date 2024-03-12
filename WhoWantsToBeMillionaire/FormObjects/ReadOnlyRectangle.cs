using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    public struct ReadOnlyRectangle
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public readonly Size Size;
        public readonly Point Location;

        public ReadOnlyRectangle(Rectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            Location = new Point(X, Y);
            Size = new Size(Width, Height);
        }
    }
}
