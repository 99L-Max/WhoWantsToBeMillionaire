using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class ButtonMainMenu : ButtonWire
    {
        public static readonly ReadOnlyDictionary<MainMenuCommand, string> TextButton;

        private MainMenuCommand command;

        public MainMenuCommand Command
        {
            set
            {
                command = value;
                Text = TextButton[value];
            }
            get => command;
        }

        static ButtonMainMenu()
        {
            var cmd = new Dictionary<MainMenuCommand, string>()
            {
                { MainMenuCommand.Start, "Новая игра" },
                { MainMenuCommand.Continue, "Продолжить игру" },
                { MainMenuCommand.Achievements, "Достижения" },
                { MainMenuCommand.Settings, "Настройки" },
                { MainMenuCommand.Exit, "Выход" }
            };

            TextButton = new ReadOnlyDictionary<MainMenuCommand, string>(cmd);
        }

        public ButtonMainMenu(float sizeFont) : base()
        {
            Font = new Font("", sizeFont, FontStyle.Bold);
            Dock = DockStyle.Fill;
            OnMouseLeave(EventArgs.Empty);
        }
    }
}
