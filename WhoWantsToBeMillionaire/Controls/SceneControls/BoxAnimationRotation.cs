using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class BoxAnimationRotation : PictureBox
    {
        public BoxAnimationRotation()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Zoom;
            SizeMode = PictureBoxSizeMode.Zoom;
        }

        protected async Task ShowAnimationRotation(Image front, Image back, Image finalFrame, (float, float, bool)[] animationData, bool isReverse = false)
        {
            if (isReverse)
                Array.Reverse(animationData);

            var imageRectangle = new Rectangle(new Point(), front.Size);

            float compression, proportion;
            bool isFront;
            Rectangle frameRectangle;

            using (var frame = new Bitmap(front))
            using (var g = Graphics.FromImage(frame))
            {
                foreach (var data in animationData)
                {
                    (compression, proportion, isFront) = data;

                    frameRectangle = Resizer.ResizeRectangle(imageRectangle, compression * proportion, proportion);

                    g.Clear(BackColor);
                    g.DrawImage(isFront ? front : back, frameRectangle);

                    Image = frame;

                    await Task.Delay(GameConst.DeltaTime);
                }

                Image = finalFrame;
            }
        }
    }
}
