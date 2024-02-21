using System.Windows.Forms;
using System.Drawing;
using System;

namespace WhoWantsToBeMillionaire
{
    enum MainMenuCommand
    {
        Start,
        Continue,
        Achievements,
        Settings,
        Exit
    }

    class MainMenu : PictureBox
    {
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(MainMenuCommand command);
        public event EventButtonClick ButtonClick;

        public bool TableVisible
        {
            set => table.Visible = value;
            get => table.Visible;
        }

        public MainMenu()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            table = new TableLayoutPanel();
            table.BackColor = Color.Transparent;

            Controls.Add(table);
        }

        public void SetCommands(MainMenuCommand[] commands)
        {
            foreach (var ctrl in table.Controls)
                if (ctrl is ButtonMainMenu)
                    (ctrl as ButtonMainMenu).Dispose();

            int heightButton = (int)(0.08f * MainForm.RectScreen.Height);

            table.Controls.Clear();
            table.ColumnStyles.Clear();
            table.RowStyles.Clear();

            table.RowCount = commands.Length;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1f));

            table.Size = new Size(MainForm.RectScreen.Width, heightButton * commands.Length);
            table.Location = new Point((MainForm.RectScreen.Width - table.Width) >> 1, (MainForm.RectScreen.Height - table.Height) >> 1);

            ButtonMainMenu[] buttons = new ButtonMainMenu[commands.Length];

            float fontSize = 0.32f * heightButton;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonMainMenu(fontSize);
                buttons[i].Command = commands[i];
                buttons[i].Click += OnButtonClick;

                table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                table.Controls.Add(buttons[i], 0, i);
            }
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            ButtonClick.Invoke((sender as ButtonMainMenu).Command);
        }
    }
}
