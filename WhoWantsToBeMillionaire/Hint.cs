using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum StatusHint
    {
        Active,
        Used,
        Locked
    }

    enum TypeHint
    {
        FiftyFifty,
        Call,
        Audience,
        RightMistake,
        Replace,
        Host
    }

    class Hint : PictureBox, IDisposable
    {
        private static readonly Bitmap background;

        private readonly Bitmap image;
        
        private bool isShown;

        public readonly TypeHint Type;

        public StatusHint Status { private set; get; }

        static Hint()
        {
            background = new Bitmap(ResourceProcessing.GetImage("Focus_Hint.png"));
        }

        public Hint(TypeHint type)
        {
            Type = type;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            image = new Bitmap(ResourceProcessing.GetImage($"Hint_{type}_{StatusHint.Active}.png"));

            Enabled = isShown = false;
        }

        private void SetStatus(StatusHint status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            float proporsion = 0.75f;

            using (Bitmap icon = new Bitmap(ResourceProcessing.GetImage($"Hint_{Type}_{Status}.png")))
            using (Graphics g = Graphics.FromImage(image))
            {
                float width = image.Width * proporsion;
                float height = image.Height * proporsion;

                float x = (image.Width - width) / 2f;
                float y = (image.Height - height) / 2f;

                g.Clear(Color.Transparent);
                g.DrawImage(icon, x, y, width, height);
                Image = image;
            }
        }

        private void OnHintMouseLeave(object sender, EventArgs e) => BackgroundImage = null;

        private void OnHintMouseEnter(object sender, EventArgs e) => BackgroundImage = background;

        private void OnHintClick(object sender, EventArgs e)
        {
            ClearMouseEvents();
            SetStatus(StatusHint.Used, false);
        }

        public void Lock()
        {
            ClearMouseEvents();
            SetStatus(StatusHint.Locked, false);
        }

        private void ClearMouseEvents()
        {
            OnHintMouseLeave(this, EventArgs.Empty);
            Click -= OnHintClick;
            MouseEnter -= OnHintMouseEnter;
            MouseLeave -= OnHintMouseLeave;
        }

        public new async void Show()
        {
            if (isShown) return;

            isShown = true;

            using (Stream stream = ResourceProcessing.GetStream("ShowHint.json", TypeResource.AnimationData))
            using (StreamReader reader = new StreamReader(stream))
            using (Bitmap icon = new Bitmap(image))
            using (Bitmap reverseSide = new Bitmap(ResourceProcessing.GetImage("ReverseSide_Hint.png")))
            using (Graphics g = Graphics.FromImage(image))
            {
                string jsonText = reader.ReadToEnd();
                (float, float, bool)[] data = JsonConvert.DeserializeObject<(float, float, bool)[]>(jsonText);

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

                SetStatus(StatusHint.Active, true);
            }

            MouseEnter += OnHintMouseEnter;
            MouseLeave += OnHintMouseLeave;
            Click += OnHintClick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearMouseEvents();
                Image = null;
                image.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
