namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public readonly MainMenuCommand Command;

        public ButtonMainMenu(MainMenuCommand cmd) : base()
        {
            Command = cmd;
        }
    }
}
