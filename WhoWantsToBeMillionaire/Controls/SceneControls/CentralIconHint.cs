using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class CentralIconHint : BoxAnimationRotation
    {
        private readonly Stack<Image> _icons;

        public CentralIconHint() : base()
        {
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
                GameSound.Play(isShow ? Resources.CentralIcon_Show : Resources.CentralIcon_Hide);

            var data = JsonManager.GetObject<(float, float, bool)[]>(Resources.AnimationData_CentralIcon);

            using (var back = Resources.Hint_ReverseSide)
                await ShowAnimationRotation(icon, back, isShow ? icon : null, data, !isShow);
        }

        public async Task ShowIcon(TypeHint type, bool playSound)
        {
            var icon = Painter.CutSprite(Resources.Hint_Icons, 3, 6, 0, (int)type);

            await ShowAnimation(icon, true, playSound);

            BackgroundImage = icon;
            Image = null;

            _icons.Push(icon);
        }

        public async Task HideIcon(bool playSound)
        {
            if (_icons.Count == 0) return;

            var icon = _icons.Pop();

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
