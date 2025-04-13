namespace WhoWantsToBeMillionaire
{
    class MenuExit : ContextMenu
    {
        private readonly ButtonContextMenu _buttonExit;

        public MenuExit(float fractionScreenHeight, int widthFraction, int heightFraction) :
            base("Вы хотите выйти из игры?", fractionScreenHeight, widthFraction, heightFraction, 0.1f)
        {
            _buttonExit = new ButtonContextMenu(ContextMenuCommand.Exit);
            _buttonExit.Text = "Выйти из игры";
            _buttonExit.Click += OnButtonClick;

            SetControls(_buttonExit);
            SetHeights(1);
        }
    }
}
