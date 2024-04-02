using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Text;
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

        private GameToolTip _toolTip = null;
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
                    _toolTip = new GameToolTip(350, 90, 14f);
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

        public ButtonHint(TypeHint type, string description, bool toolTipVisible)
        {
            Type = type;
            Description = description;
            ToolTipVisible = toolTipVisible;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            Enabled = _isShown = false;

            _image = (Image)Resources.ResourceManager.GetObject($"Hint_{type}_{StatusHint.Active}");
        }

        private void SetStatus(StatusHint status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            float proporsion = 0.75f;

            using (Image icon = (Image)Resources.ResourceManager.GetObject($"Hint_{Type}_{Status}"))
            using (Graphics g = Graphics.FromImage(_image))
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

        private void OnHintMouseLeave(object sender, EventArgs e) =>
            BackgroundImage = null;

        private void OnHintMouseEnter(object sender, EventArgs e) =>
            BackgroundImage = s_focus;

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

            MouseEnter += OnHintMouseEnter;
            MouseLeave += OnHintMouseLeave;
            Click += OnHintClick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearMouseEvents();

                _image.Dispose();
                _toolTip?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
