using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum StatusHint { Active, Used, Locked }

    enum TypeHint { FiftyFifty, PhoneFriend, AskAudience, DoubleDip, SwitchQuestion, AskHost }

    class ButtonHint : PictureBox, IDisposable
    {
        private static readonly Image s_focus;

        private readonly Image _image;

        private GameToolTip _toolTip;

        public readonly TypeHint Type;
        public readonly string Description;

        static ButtonHint() =>
            s_focus = Resources.Focus_Hint;

        public ButtonHint(TypeHint type, string description)
        {
            Type = type;
            Description = description;
            Enabled = IsShown = false;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            _image = (Image)Resources.ResourceManager.GetObject($"Hint_{type}_{StatusHint.Active}");
        }

        public bool IsShown { get; private set; }

        public StatusHint Status { get; private set; }

        public bool ToolTipVisible
        {
            set => SetToolTipVisible(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image.Dispose();
                _toolTip?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SetToolTipVisible(bool visible)
        {
            if (visible && _toolTip == null)
            {
                _toolTip = new GameToolTip(300, 120, 3, 16f);
                _toolTip.SetToolTip(this, Description);
            }
            else
            {
                _toolTip?.Dispose();
                _toolTip = null;
            }
        }

        private void SetStatus(StatusHint status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            float proportion = 0.75f;

            using (var icon = (Image)Resources.ResourceManager.GetObject($"Hint_{Type}_{Status}"))
            using (var g = Graphics.FromImage(_image))
            {
                float width = _image.Width * proportion;
                float height = _image.Height * proportion;

                float x = (_image.Width - width) / 2f;
                float y = (_image.Height - height) / 2f;

                g.Clear(Color.Transparent);
                g.DrawImage(icon, x, y, width, height);

                Image = _image;
            }
        }

        protected override void OnMouseLeave(EventArgs e) =>
            BackgroundImage = null;

        protected override void OnMouseEnter(EventArgs e) =>
            BackgroundImage = s_focus;

        protected override void OnClick(EventArgs e)
        {
            SetStatus(StatusHint.Used, false);
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!Enabled)
                OnMouseLeave(e);
        }

        public async void ShowIcon()
        {
            if (IsShown) return;

            IsShown = true;

            using (var icon = new Bitmap(_image))
            using (var reverseSide = Resources.ReverseSide_Hint)
            using (var g = Graphics.FromImage(_image))
            {
                var data = JsonManager.GetObject<(float, float, bool)[]>(Resources.AnimationData_ButtonHint);

                bool isFront;
                float compression, proportion;
                float x, y, width, height;

                foreach (var el in data)
                {
                    (compression, proportion, isFront) = el;

                    width = _image.Width * compression * proportion;
                    height = _image.Height * proportion;

                    x = (_image.Width - width) / 2f;
                    y = (_image.Height - height) / 2f;

                    g.Clear(BackColor);
                    g.DrawImage(isFront ? icon : reverseSide, x, y, width, height);

                    Image = _image;

                    await Task.Delay(MainForm.DeltaTime);
                }

                SetStatus(StatusHint.Active, true);
            }
        }

        public void Lock() =>
            SetStatus(StatusHint.Locked, false);
    }
}
