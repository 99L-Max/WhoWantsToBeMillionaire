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
        private readonly Stack<Bitmap> icons;

        public CentralIconHint()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;

            icons = new Stack<Bitmap>();
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

        public async Task ShowIcon(TypeHint type)
        {
            Bitmap icon = new Bitmap(ResourceProcessing.GetImage($"Hint_{type}_{StatusHint.Active}.png"));

            await ShowAnimation(icon, true);

            BackgroundImage = icon;
            icons.Push(icon);
        }

        public async Task HideIcon()
        {
            if (icons.Count == 0) return;

            Bitmap icon = icons.Pop();

            BackgroundImage = icons.Count > 0 ? icons.Peek() : null;

            await ShowAnimation(icon, false);

            icon.Dispose();
        }

        public void Clear()
        {
            BackgroundImage = Image = null;

            while (icons.Count > 0);
                icons.Pop().Dispose();
        }
    }
}
