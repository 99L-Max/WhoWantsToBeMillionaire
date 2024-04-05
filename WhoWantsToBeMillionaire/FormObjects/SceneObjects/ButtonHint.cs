using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

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
        private static readonly Image s_focus;

        private readonly Image _image;

        private GameToolTip _toolTip;
        private bool _isShown;

        public readonly TypeHint Type;
        public readonly string Description;

        public StatusHint Status { private set; get; }

        public bool ToolTipVisible
        {
            set
            {
                if (value && _toolTip == null)
                {
                    _toolTip = new GameToolTip(300, 120, 16f);
                    _toolTip.SetToolTip(this, Description);
                }
                else
                {
                    _toolTip?.Dispose();
                    _toolTip = null;
                }
            }
        }

        static ButtonHint() =>
            s_focus = Resources.Focus_Hint;

        public ButtonHint(TypeHint type, string description)
        {
            Type = type;
            Description = description;
            Enabled = _isShown = false;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            _image = (Image)Resources.ResourceManager.GetObject($"Hint_{type}_{StatusHint.Active}");
        }

        private void SetStatus(StatusHint status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            float proporsion = 0.75f;

            using (var icon = (Image)Resources.ResourceManager.GetObject($"Hint_{Type}_{Status}"))
            using (var g = Graphics.FromImage(_image))
            {
                float width = _image.Width * proporsion;
                float height = _image.Height * proporsion;

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

        protected override void OnClick(EventArgs e) =>
            SetStatus(StatusHint.Used, false);

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!Enabled)
                OnMouseLeave(e);
        }

        public void Lock() =>
            SetStatus(StatusHint.Locked, false);

        public async void ShowIcon()
        {
            if (_isShown) return;

            _isShown = true;

            using (Image icon = new Bitmap(_image))
            using (Image reverseSide = Resources.ReverseSide_Hint)
            using (Graphics g = Graphics.FromImage(_image))
            {
                var data = JsonManager.GetObject<(float, float, bool)[]>(Resources.AnimationData_ButtonHint);

                bool isFront;
                float compression, proporsion;
                float x, y, width, height;

                foreach (var el in data)
                {
                    (compression, proporsion, isFront) = el;

                    width = _image.Width * compression * proporsion;
                    height = _image.Height * proporsion;

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image.Dispose();
                _toolTip?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
