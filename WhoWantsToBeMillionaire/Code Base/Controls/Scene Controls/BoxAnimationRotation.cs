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

        protected async Task ShowAnimationRotation(Image front, Image back, Image finalFrame, AnimationRotationData[] animationData, bool isReverse = false)
        {
            if (isReverse)
            { 
                Array.Reverse(animationData);
            }

            var imageRectangle = new Rectangle(new Point(), front.Size);

            using (var frame = new Bitmap(front))
            using (var g = Graphics.FromImage(frame))
            {
                foreach (var data in animationData)
                {
                    var frameRectangle = Resizer.ResizeRectangle(imageRectangle, data.Compression * data.Scale, data.Scale);

                    g.Clear(BackColor);
                    g.DrawImage(data.IsFront ? front : back, frameRectangle);

                    Image = frame;

                    await Task.Delay(GameConst.DeltaTime);
                }

                Image = finalFrame;
            }
        }
    }
}
