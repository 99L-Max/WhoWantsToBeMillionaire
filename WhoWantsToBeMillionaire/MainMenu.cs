using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MainMenu : Menu
    {
        public MainMenu(Size size) : base(size)
        {
            ButtonCommand[] cmd = { ButtonCommand.Start, ButtonCommand.Statistics, ButtonCommand.Achievements, ButtonCommand.Settings, ButtonCommand.Exit };
            CustomButton[] buttons = new CustomButton[cmd.Length];

            float fontSize = 0.25f * size.Height / cmd.Length;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new CustomButton(fontSize);
                buttons[i].Command = cmd[i];
                buttons[i].Click += (s, e) => OnButtonClick((s as CustomButton).Command);
            }

            SetControls(buttons);
        }
    }
}
