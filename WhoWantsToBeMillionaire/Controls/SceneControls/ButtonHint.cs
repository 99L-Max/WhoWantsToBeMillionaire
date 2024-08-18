using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum StatusHint { Active, Used, Locked }

    enum TypeHint { FiftyFifty, PhoneFriend, AskAudience, DoubleDip, SwitchQuestion, AskHost }

    class ButtonHint : BoxAnimationRotation, IDisposable
    {
        private const float RatioIcon = 0.75f;

        private readonly Image _image;
        private readonly Image _focusImage;

        private GameToolTip _toolTip;

        public readonly TypeHint Type;
        public readonly string Description;

        public ButtonHint(TypeHint type, string description)
        {
            Type = type;
            Description = description;
            Enabled = IsShown = false;

            BackColor = Color.Transparent;
            SizeMode = PictureBoxSizeMode.Zoom;
            BackgroundImageLayout = ImageLayout.Zoom;

            _image = Painter.CutSprite(Resources.Hint_Icons, 3, 6, 0, (int)Type);
            _focusImage = Painter.CreateGradientEllipse(_image.Size, Color.White, 0.5f);
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
                _focusImage.Dispose();
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

            var rectangle = new Rectangle(0, 0, _image.Width, _image.Height);

            using (var icon = Painter.CutSprite(Resources.Hint_Icons, 3, 6, (int)status, (int)Type))
            using (var g = Graphics.FromImage(_image))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(icon, Resizer.ResizeRectangle(rectangle, RatioIcon));

                Image = _image;
            }
        }

        protected override void OnMouseLeave(EventArgs e) =>
            BackgroundImage = null;

        protected override void OnMouseEnter(EventArgs e) =>
            BackgroundImage = _focusImage;

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

            var data = JsonManager.GetObject<(float, float, bool)[]>(Resources.AnimationData_ButtonHint);

            using (var finalFrame = Resizer.ResizeImage(_image, RatioIcon))
            using (var back = Resources.Hint_ReverseSide)
            {
                await ShowAnimationRotation(_image, back, finalFrame, data);

                SetStatus(StatusHint.Active, true);
            }
        }

        public void Lock() =>
            SetStatus(StatusHint.Locked, false);
    }
}
