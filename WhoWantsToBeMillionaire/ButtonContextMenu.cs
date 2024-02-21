namespace WhoWantsToBeMillionaire
{
    class ButtonContextMenu : ButtonEllipse
    {
        public readonly ContextMenuCommand Command;

        public ButtonContextMenu(ContextMenuCommand cmd, string text, float fontSize) : base(text, fontSize)
        {
            Command = cmd;
        }
    }
}
