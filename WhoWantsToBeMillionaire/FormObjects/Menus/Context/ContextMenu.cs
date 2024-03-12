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

    abstract class ContextMenu : PictureBox, IDisposable
    {
        protected readonly TableLayoutPanel table;

        public delegate void EventButtonClick(ContextMenuCommand command);
        public event EventButtonClick ButtonClick;

        public ContextMenu(int width, int height)
        {
            Size = new Size(width, height);
            Location = new Point((MainForm.RectScreen.Width - Width) >> 1, (MainForm.RectScreen.Height - Height) >> 1);
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = new Bitmap(ResourceManager.GetImage("Menu.png"), Size);

            table = new TableLayoutPanel();

            table.Size = new Size((int)(0.9f * Width), (int)(0.9f * Height));
            table.Location = new Point((Width - table.Width) >> 1, (Height - table.Height) >> 1);

            Controls.Add(table);
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            ButtonClick.Invoke((sender as ButtonContextMenu).Command);
        }

        protected void SetHeights(params float[] heights)
        {
            table.RowStyles.Clear();
            table.RowCount = heights.Length;

            foreach (var h in heights)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, h));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control ctrl in table.Controls)
                {
                    if(ctrl is ButtonContextMenu)
                        (ctrl as ButtonContextMenu).Click -= OnButtonClick;

                    if (ctrl is IDisposable) 
                        (ctrl as IDisposable).Dispose();
                }

                table.Controls.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
