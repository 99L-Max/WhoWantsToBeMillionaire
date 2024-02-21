using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ContextMenuCommand
    {
        Back,
        StartGame
    }

    abstract class ContextMenu : PictureBox
    {
        protected readonly TableLayoutPanel table;

        public delegate void EventButtonClick(ContextMenuCommand command);
        public event EventButtonClick ButtonClick;

        public ContextMenu(Size size)
        {
            Size = size;
            Location = new Point((MainForm.RectScreen.Width - Width) >> 1, (MainForm.RectScreen.Height - Height) >> 1);
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Menu.png"), size);

            table = new TableLayoutPanel();

            table.Size = new Size((int)(0.9f * Width), (int)(0.9f * Height));
            table.Location = new Point((Width - table.Width) >> 1, (Height - table.Height) >> 1);

            Controls.Add(table);
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            ButtonClick.Invoke((sender as ButtonContextMenu).Command);
        }
    }
}
