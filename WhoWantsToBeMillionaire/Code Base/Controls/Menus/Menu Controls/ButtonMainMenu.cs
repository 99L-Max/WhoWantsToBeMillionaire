namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public ButtonMainMenu(MainMenuCommands command) : base()
        {
            Command = command;
        }

        public MainMenuCommands Command { get; }
    }
}
