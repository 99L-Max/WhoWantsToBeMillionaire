using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class CentralIconHint : PictureBox
    {
        private readonly Stack<Bitmap> backgrounds;

        public CentralIconHint()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;

            backgrounds = new Stack<Bitmap>();
        }

        private async Task ShowAnimation(Bitmap icon, bool isShow)
        {
            using (Stream stream = ResourceProcessing.GetStream("ShowCentralIcon.json", TypeResource.AnimationData))
            using (StreamReader reader = new StreamReader(stream))
            using (Bitmap reverseSide = new Bitmap(ResourceProcessing.GetImage("ReverseSide_Hint.png")))
            using (Bitmap image = new Bitmap(icon.Width, icon.Height))
            using (Graphics g = Graphics.FromImage(image))
            {
                string jsonText = reader.ReadToEnd();
                (float, float, bool)[] data = JsonConvert.DeserializeObject<(float, float, bool)[]>(jsonText);

                if (!isShow)
                    Array.Reverse(data);

                float compression, proporsion;
                bool isFront;

                float x, y, width, height;

                foreach (var el in data)
                {
                    (compression, proporsion, isFront) = el;

                    width = image.Width * compression * proporsion;
                    height = image.Height * proporsion;

                    x = (image.Width - width) / 2f;
                    y = (image.Height - height) / 2f;

                    g.Clear(BackColor);
                    g.DrawImage(isFront ? icon : reverseSide, x, y, width, height);

                    Image = image;

                    await Task.Delay(MainForm.DeltaTime);
                }

                Image = null;
            }
        }

        public async void ShowIcon(TypeHint type)
        {
            Bitmap icon = new Bitmap(ResourceProcessing.GetImage($"Hint_{type}_{StatusHint.Active}.png"));

            await ShowAnimation(icon, true);

            BackgroundImage = icon;
            backgrounds.Push(icon);
        }

        public async void HideIcon()
        {
            if (backgrounds.Count == 0) return;

            Bitmap icon = backgrounds.Pop();

            BackgroundImage = backgrounds.Count > 0 ? backgrounds.Peek() : null;

            await ShowAnimation(icon, false);

            icon.Dispose();
        }

        public async Task Clear(int countFrames)
        {
            if (backgrounds.Count == 0) return;

            Bitmap[] frames = ResourceProcessing.FramesDisappearance(backgrounds.Peek(), countFrames);
            BackgroundImage = null;

            foreach (var img in frames)
            {
                Image = img;
                await Task.Delay(MainForm.DeltaTime);
            }

            Image = null;

            foreach (var img in frames)
                img.Dispose();

            do
                backgrounds.Pop().Dispose();
            while (backgrounds.Count > 0);
        }
    }
}
