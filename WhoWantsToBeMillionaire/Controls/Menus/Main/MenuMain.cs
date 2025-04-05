using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuMain : PictureBox
    {
        private readonly TableLayoutPanel _table;

        public Action<MainMenuCommand> ButtonClick;

        public MenuMain()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            _table = new TableLayoutPanel { BackColor = Color.Transparent };

            Controls.Add(_table);
        }

        public bool ButtonsVisible
        {
            get => _table.Visible;
            set => _table.Visible = value;
        }

        public static MainMenuCommand[] GetCommands =>
            Enum.GetValues(typeof(MainMenuCommand)).Cast<MainMenuCommand>().ToArray();

        public void SetCommands(params MainMenuCommand[] commands)
        {
            foreach (Control ctrl in _table.Controls)
            {
                if (ctrl is ButtonMainMenu btn)
                    btn.Click -= OnButtonClick;

                ctrl.Dispose();
            }

            var heightButton = (int)(0.08f * GameConst.ScreenSize.Height);

            _table.Controls.Clear();
            _table.RowStyles.Clear();

            _table.Size = new Size(GameConst.ScreenSize.Width, heightButton * commands.Length);
            _table.Location = new Point(GameConst.ScreenSize.Width - _table.Width >> 1, GameConst.ScreenSize.Height - _table.Height >> 1);

            var buttons = new ButtonMainMenu[commands.Length];
            var dict = JsonManager.GetDictionary<MainMenuCommand, string>(Resources.Dictionary_MenuCommands);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonMainMenu(commands[i]) { Text = dict[commands[i]] };
                buttons[i].Click += OnButtonClick;

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                _table.Controls.Add(buttons[i], 0, i);
            }

            foreach (var b in buttons)
                b.AlignSize();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender is ButtonMainMenu btn)
                ButtonClick?.Invoke(btn.Command);
        }
    }
}