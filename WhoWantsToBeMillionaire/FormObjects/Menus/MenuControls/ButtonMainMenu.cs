namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public readonly MainMenuCommand Command;

        public ButtonMainMenu(MainMenuCommand cmd, float fontSize) : base(fontSize)
        {
            Command = cmd;
        }
    }
}
