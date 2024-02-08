using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WhoWantsToBeMillionaire
{
    enum TypeResource
    {
        AnimationData,
        Dialogues,
        Questions,
        Sounds,
        Sums,
        Textures
    }

    static class ResourceProcessing
    {
        public static Stream GetStream(string resourceName, TypeResource type)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"WhoWantsToBeMillionaire.Resources.{type}.{resourceName}");
        }

        public static Image GetImage(string resourceName)
        {
            using (Stream stream = GetStream(resourceName, TypeResource.Textures))
                return Image.FromStream(stream);
        }

        public static Bitmap[] FramesTransition(Bitmap sourceFrame, Bitmap finalFrame, int count)
        {
            Bitmap[] bitmaps = Enumerable.Range(0, count).Select(x => new Bitmap(sourceFrame)).ToArray();
            Bitmap[] frames = FramesAppearance(finalFrame, count);

            Rectangle rect = new Rectangle(0, 0, sourceFrame.Width, sourceFrame.Height);

            for (int i = 0; i < bitmaps.Length; i++)
            {
                using (Graphics g = Graphics.FromImage(bitmaps[i]))
                    g.DrawImage(frames[i], rect);

                frames[i].Dispose();
            }

            return bitmaps;
        }

        public static Bitmap[] FramesAppearance(Bitmap image, int count)
        {
            Bitmap[] bitmaps = Enumerable.Range(0, count).Select(x => new Bitmap(image.Width, image.Height)).ToArray();

            for (int i = 0; i < bitmaps.Length; i++)
            {
                using (Graphics g = Graphics.FromImage(bitmaps[i]))
                using (ImageAttributes attributes = new ImageAttributes())
                {
                    ColorMatrix matrix = new ColorMatrix { Matrix33 = (i + 1f) / bitmaps.Length };
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return bitmaps;
        }

        public static Bitmap[] FramesDisappearance(Bitmap image, int count)
        {
            Bitmap[] frames = FramesAppearance(image, count);
            Array.Reverse(frames);
            return frames;
        }

        public static string GetString(string fileName)
        {
            using (StreamReader stream = new StreamReader(GetStream(fileName, TypeResource.Dialogues)))
                return stream.ReadToEnd();
        }
    }
}
