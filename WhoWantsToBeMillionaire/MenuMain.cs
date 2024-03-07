using System;
using System.Drawing;
using System.Windows.Forms;

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
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(MainMenuCommand command);
        public event EventButtonClick ButtonClick;

        public bool ButtonsVisible
        {
            set => table.Visible = value;
            get => table.Visible;
        }

        public MenuMain()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            table = new TableLayoutPanel();
            table.BackColor = Color.Transparent;

            Controls.Add(table);
        }

        public void SetCommands(params MainMenuCommand[] commands)
        {
            foreach (var ctrl in table.Controls)
                if (ctrl is IDisposable)
                    (ctrl as IDisposable).Dispose();

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
            var dict = ResourceProcessing.GetDictionary("MenuCommands.json");

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonMainMenu(commands[i], fontSize);
                buttons[i].Text = dict[commands[i].ToString()];
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
