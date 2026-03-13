using System;
using System.Drawing;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class HintButton : BoxAnimationRotation, IDisposable
    {
        private const float RatioIcon = 0.75f;

        private readonly Image _image;
        private readonly Image _focusImage;

        private GameToolTip _toolTip;

        public HintButton(HintTypes type, string description) : base()
        {
            Type = type;
            Description = description;
            Enabled = IsShown = false;

            _image = SpritePainter.GetSprite(Resources.Hint_Icons, 3, 6, 0, (int)Type);
            _focusImage = Painter.GetGradientEllipse(_image.Size, Color.White, 0.5f);
        }

        public HintTypes Type { get; }

        public string Description { get; }

        public bool IsShown { get; private set; }

        public HintStatuses Status { get; private set; }

        public bool ToolTipVisible { set => SetToolTipVisible(value); }

        public async void ShowIcon()
        {
            if (IsShown) return;

            IsShown = true;

            var data = JsonReader.GetObject<AnimationRotationData[]>(Resources.AnimationData_ButtonHint);

            using (var finalFrame = Resizer.ResizeImage(_image, RatioIcon))
            using (var back = Resources.Hint_ReverseSide)
            {
                await ShowAnimationRotation(_image, back, finalFrame, data);
                SetStatus(HintStatuses.Active, true);
            }
        }

        public void Lock()
        {
            SetStatus(HintStatuses.Locked, false);
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

        protected override void OnMouseLeave(EventArgs e)
        {
            BackgroundImage = null;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            BackgroundImage = _focusImage;
        }

        protected override void OnClick(EventArgs e)
        {
            SetStatus(HintStatuses.Used, false);
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (Enabled == false)
                OnMouseLeave(e);
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

        private void SetStatus(HintStatuses status, bool enabled)
        {
            Status = status;
            Enabled = enabled;

            var rectangle = new Rectangle(0, 0, _image.Width, _image.Height);

            using (var icon = SpritePainter.GetSprite(Resources.Hint_Icons, 3, 6, (int)status, (int)Type))
            using (var g = Graphics.FromImage(_image))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(icon, Resizer.ResizeRectangle(rectangle, RatioIcon));

                Image = _image;
            }
        }
    }
}
