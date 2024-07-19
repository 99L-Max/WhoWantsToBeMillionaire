namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public readonly MainMenuCommand Command;

        public ButtonMainMenu(MainMenuCommand command) : base()
        {
            Command = command;
        }
    }
}
