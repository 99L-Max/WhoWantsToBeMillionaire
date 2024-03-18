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
        PhoneFriend,
        AskAudience,
        DoubleDip,
        SwitchQuestion,
        AskHost
    }

    class ButtonHint : PictureBox, IDisposable
    {
        private static readonly Image focus;

        private readonly Image image;

        private GameToolTip toolTip = null;
        private bool isShown;

        public readonly TypeHint Type;
        public readonly string Description;

        public StatusHint Status { private set; get; }

        public bool ToolTipVisible
        {
            set
            {
                if (value && toolTip == null)
                {
                    toolTip = new GameToolTip(350, 90, 14f);
                    toolTip.SetToolTip(this, Description);
                }
                else
                {
                    toolTip?.Dispose();
                    toolTip = null;
                }
            }
        }

        static ButtonHint() => focus = ResourceManager.GetImage("Focus_Hint.png");

        public ButtonHint(TypeHint type, string description, bool toolTipVisible)
        {
            Type = type;
            Description = description;
            ToolTipVisible = toolTipVisible;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            Enabled = isShown = false;

            image = ResourceManager.GetImage($"Hint_{type}_{StatusHint.Active}.png");
        }

        private void SetStatus(StatusHint status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            float proporsion = 0.75f;

            using (Image icon = ResourceManager.GetImage($"Hint_{Type}_{Status}.png"))
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

        private void OnHintMouseEnter(object sender, EventArgs e) => BackgroundImage = focus;

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
            Click -= OnHintClick;
            MouseEnter -= OnHintMouseEnter;
            MouseLeave -= OnHintMouseLeave;
            BackgroundImage = null;
        }

        public async void ShowIcon()
        {
            if (isShown) return;

            isShown = true;

            using (Stream stream = ResourceManager.GetStream("ShowHint.json", TypeResource.AnimationData))
            using (StreamReader reader = new StreamReader(stream))
            using (Image icon = new Bitmap(image))
            using (Image reverseSide = ResourceManager.GetImage("ReverseSide_Hint.png"))
            using (Graphics g = Graphics.FromImage(image))
            {
                (float, float, bool)[] aniData = JsonConvert.DeserializeObject<(float, float, bool)[]>(reader.ReadToEnd());

                float compression, proporsion;
                bool isFront;

                float x, y, width, height;

                foreach (var data in aniData)
                {
                    (compression, proporsion, isFront) = data;

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

                image.Dispose();
                toolTip?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
