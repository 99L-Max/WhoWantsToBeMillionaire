using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MainMenu : Menu
    {
        public MainMenu(Size size) : base(size)
        {
            MenuCommand[] cmd = { MenuCommand.Start, MenuCommand.Statistics, MenuCommand.Achievements, MenuCommand.Settings, MenuCommand.Exit };
            MenuButton[] buttons = new MenuButton[cmd.Length];

            float fontSize = 0.25f * size.Height / cmd.Length;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new MenuButton(fontSize);
                buttons[i].Command = cmd[i];
                buttons[i].Click += (s, e) => OnButtonClick((s as MenuButton).Command);
            }

            SetControls(buttons);
        }
    }
}
