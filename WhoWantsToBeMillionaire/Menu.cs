using System.Windows.Forms;
using System.Drawing;
using System;

namespace WhoWantsToBeMillionaire
{
    class Menu : PictureBox
    {
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(MenuCommand command);
        public event EventButtonClick ButtonClick;

        public Menu(Size size)
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(byte.MaxValue >> 1, Color.Black);

            table = new TableLayoutPanel();
            table.Size = size;
            table.BackColor = Color.Transparent;
            table.Location = new Point((MainForm.RectScreen.Width - size.Width) / 2, (MainForm.RectScreen.Height - size.Height) / 2);

            Controls.Add(table);
        }

        public void SetCommands(MenuCommand[] commands)
        {
            foreach (var ctrl in Controls)
                if (ctrl is ButtonMenu)
                    (ctrl as ButtonMenu).Dispose();

            table.Controls.Clear();
            table.ColumnStyles.Clear();
            table.RowStyles.Clear();

            table.RowCount = commands.Length;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1f));

            ButtonMenu[] buttons = new ButtonMenu[commands.Length];

            float fontSize = 0.25f * table.Height / commands.Length;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonMenu(fontSize);
                buttons[i].Command = commands[i];
                buttons[i].Click += OnButtonClick;

                table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                table.Controls.Add(buttons[i], 0, i);
            }
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            ButtonClick.Invoke((sender as ButtonMenu).Command);
        }
    }
}
