using System;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum MainMenuCommand
    {
        NewGame,
        Continue,
        Achievements,
        Settings,
        Statistics,
        Exit
    }

    class MenuMain : PictureBox
    {
        private readonly TableLayoutPanel _table;

        public delegate void EventButtonClick(MainMenuCommand command);
        public event EventButtonClick ButtonClick;

        public bool ButtonsVisible
        {
            set => _table.Visible = value;
            get => _table.Visible;
        }

        public MenuMain()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            _table = new TableLayoutPanel();
            _table.BackColor = Color.Transparent;

            Controls.Add(_table);
        }

        public void SetCommands(params MainMenuCommand[] commands)
        {
            foreach (Control ctrl in _table.Controls)
                ctrl.Dispose();

            int heightButton = (int)(0.08f * MainForm.ScreenRectangle.Height);

            _table.Controls.Clear();
            _table.RowStyles.Clear();

            _table.Size = new Size(MainForm.ScreenRectangle.Width, heightButton * commands.Length);
            _table.Location = new Point((MainForm.ScreenRectangle.Width - _table.Width) >> 1, (MainForm.ScreenRectangle.Height - _table.Height) >> 1);

            ButtonMainMenu[] buttons = new ButtonMainMenu[commands.Length];

            var fontSize = 0.4f * heightButton;
            var dict = JsonManager.GetDictionary<MainMenuCommand, string>(Resources.Dictionary_MenuCommands);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonMainMenu(commands[i], fontSize);
                buttons[i].Text = dict[commands[i]];
                buttons[i].Click += OnButtonClick;

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                _table.Controls.Add(buttons[i], 0, i);
            }
        }

        protected void OnButtonClick(object sender, EventArgs e) =>
            ButtonClick.Invoke((sender as ButtonMainMenu).Command);
    }
}
