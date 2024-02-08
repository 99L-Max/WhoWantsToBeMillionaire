using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum MenuCommand
    {
        Start,
        Continue,
        Achievements,
        Settings,
        Exit
    }

    class ButtonMenu : CustomButton
    {
        public static readonly ReadOnlyDictionary<MenuCommand, string> TextButton;

        private MenuCommand command;

        public MenuCommand Command
        {
            set
            {
                command = value;
                Text = TextButton[value];
            }
            get => command;
        }

        static ButtonMenu()
        {
            var cmd = new Dictionary<MenuCommand, string>()
            {
                { MenuCommand.Start, "Новая игра" },
                { MenuCommand.Continue, "Продолжить игру" },
                { MenuCommand.Achievements, "Достижения" },
                { MenuCommand.Settings, "Настройки" },
                { MenuCommand.Exit, "Выход" }
            };

            TextButton = new ReadOnlyDictionary<MenuCommand, string>(cmd);
        }

        public ButtonMenu(float sizeFont) : base()
        {
            Font = new Font("", sizeFont, FontStyle.Bold);
            Dock = DockStyle.Fill;
            OnMouseLeave(EventArgs.Empty);
        }
    }
}
