using System;

namespace WhoWantsToBeMillionaire
{
    class MenuExit : ContextMenu, IDisposable
    {
        private readonly ButtonContextMenu _buttonExit;

        public MenuExit(int width, int height) : base("Вы хотите выйти из игры?", width, height, 0.1f * height)
        {
            _buttonExit = new ButtonContextMenu(ContextMenuCommand.Exit);
            _buttonExit.Text = "Выйти из игры";
            _buttonExit.Click += OnButtonClick;

            SetControls(_buttonExit);
            SetHeights(1f);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _buttonExit.Dispose();

            base.Dispose(disposing);
        }
    }
}
