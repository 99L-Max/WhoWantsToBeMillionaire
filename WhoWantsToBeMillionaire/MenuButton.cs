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
        Restart,
        Continue,
        Statistics,
        Achievements,
        Settings,
        Exit
    }

    class MenuButton : CustomButton
    {
        private MenuCommand command;

        public static readonly ReadOnlyDictionary<MenuCommand, string> TextButton;

        public MenuCommand Command
        {
            set
            {
                command = value;
                Text = TextButton[value];
            }
            get => command;
        }

        static MenuButton()
        {
            var cmd = new Dictionary<MenuCommand, string>()
            {
                { MenuCommand.Start, "Новая игра" },
                { MenuCommand.Restart, "Новая игра" },
                { MenuCommand.Continue, "Продолжить игру" },
                { MenuCommand.Statistics, "Статистика" },
                { MenuCommand.Achievements, "Достижения" },
                { MenuCommand.Settings, "Настройки" },
                { MenuCommand.Exit, "Выход" }
            };

            TextButton = new ReadOnlyDictionary<MenuCommand, string>(cmd);
        }

        public MenuButton(float sizeFont) : base()
        {
            Font = new Font("", sizeFont, FontStyle.Bold);
            Dock = DockStyle.Fill;
            OnMouseLeave(EventArgs.Empty);
        }
    }
}
