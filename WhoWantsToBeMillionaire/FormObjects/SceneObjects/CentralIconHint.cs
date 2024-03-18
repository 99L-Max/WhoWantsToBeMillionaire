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
        private readonly Stack<Image> icons;

        public CentralIconHint()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;

            icons = new Stack<Image>();
        }

        private async Task ShowAnimation(Image icon, bool isShow, bool playSound)
        {
            if (playSound)
                Sound.Play(isShow ? "CentralIcon_Show.wav" : "CentralIcon_Hide.wav");

            using (Stream stream = ResourceManager.GetStream("ShowCentralIcon.json", TypeResource.AnimationData))
            using (StreamReader reader = new StreamReader(stream))
            using (Image reverseSide = ResourceManager.GetImage("ReverseSide_Hint.png"))
            using (Image image = new Bitmap(icon.Width, icon.Height))
            using (Graphics g = Graphics.FromImage(image))
            {
                (float, float, bool)[] data = JsonConvert.DeserializeObject<(float, float, bool)[]>(reader.ReadToEnd());

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

        public async Task ShowIcon(TypeHint type, bool playSound)
        {
            Image icon = ResourceManager.GetImage($"Hint_{type}_{StatusHint.Active}.png");

            await ShowAnimation(icon, true, playSound);

            BackgroundImage = icon;
            icons.Push(icon);
        }

        public async Task HideIcon(bool playSound)
        {
            if (icons.Count == 0) return;

            Image icon = icons.Pop();

            BackgroundImage = icons.Count > 0 ? icons.Peek() : null;

            await ShowAnimation(icon, false, playSound);

            icon.Dispose();
        }

        public void Clear()
        {
            BackgroundImage = Image = null;

            while (icons.Count > 0)
                icons.Pop().Dispose();
        }
    }
}
