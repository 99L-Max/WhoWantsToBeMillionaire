using System.Windows.Forms;
using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    abstract class Menu : ControlAnimation
    {
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(ButtonCommand command);
        public event EventButtonClick ButtonClick;

        public Menu(Size size) : base(size)
        {
            Location = new Point((MainForm.RectScreen.Width - size.Width) / 2, (MainForm.RectScreen.Height - size.Height) / 2);

            table = new TableLayoutPanel();
            table.BackColor = Color.Transparent;
            table.Dock = DockStyle.Fill;

            Controls.Add(table);
        }

        protected void SetControls(Control[] controls)
        {
            table.Controls.Clear();
            table.ColumnStyles.Clear();
            table.RowStyles.Clear();

            table.RowCount = controls.Length;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1f));

            for(int i = 0; i < controls.Length; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                table.Controls.Add(controls[i], 0, i);
            }
        }

        protected void OnButtonClick(ButtonCommand cmd)
        {
            ButtonClick.Invoke(cmd);
        }
    }
}
