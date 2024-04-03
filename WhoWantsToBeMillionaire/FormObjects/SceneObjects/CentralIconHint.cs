using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class CentralIconHint : PictureBox
    {
        private readonly Stack<Image> _icons;

        public CentralIconHint()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            SizeMode = PictureBoxSizeMode.StretchImage;

            _icons = new Stack<Image>();
        }

        public void Reset()
        {
            Visible = false;
            Clear();
        }

        private async Task ShowAnimation(Image icon, bool isShow, bool playSound)
        {
            if (playSound)
                Sound.Play(isShow ? Resources.CentralIcon_Show : Resources.CentralIcon_Hide);

            using (Image reverseSide = Resources.ReverseSide_Hint)
            using (Image image = new Bitmap(icon.Width, icon.Height))
            using (Graphics g = Graphics.FromImage(image))
            {
                var data = JsonManager.GetObject<(float, float, bool)[]>(Resources.AnimationData_CentralIcon);

                if (!isShow)
                    Array.Reverse(data);

                bool isFront;
                float compression, proporsion;
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
            Image icon = (Image)Resources.ResourceManager.GetObject($"Hint_{type}_{StatusHint.Active}");

            await ShowAnimation(icon, true, playSound);

            BackgroundImage = icon;
            _icons.Push(icon);
        }

        public async Task HideIcon(bool playSound)
        {
            if (_icons.Count == 0) return;

            Image icon = _icons.Pop();

            BackgroundImage = _icons.Count > 0 ? _icons.Peek() : null;

            await ShowAnimation(icon, false, playSound);

            icon.Dispose();
        }

        public void Clear()
        {
            BackgroundImage = Image = null;

            while (_icons.Count > 0)
                _icons.Pop().Dispose();
        }
    }
}
