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
        private readonly LabelMenu labelTitle;
        private readonly ButtonContextMenu buttonBack;
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(ContextMenuCommand command);
        public event EventButtonClick ButtonClick;

        public ContextMenu(int width, int height, string title, float fontSize)
        {
            Size = new Size(width, height);
            Location = new Point((MainForm.RectScreen.Width - Width) >> 1, (MainForm.RectScreen.Height - Height) >> 1);
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = new Bitmap(ResourceManager.GetImage("Menu.png"), Size);

            table = new TableLayoutPanel();
            labelTitle = new LabelMenu(fontSize, ContentAlignment.MiddleCenter);
            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);

            labelTitle.Text = title;
            buttonBack.Text = "Назад";

            buttonBack.Click += OnButtonClick;

            table.Size = new Size((int)(0.9f * Width), (int)(0.9f * Height));
            table.Location = new Point((Width - table.Width) >> 1, (Height - table.Height) >> 1);

            Controls.Add(table);
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            ButtonClick.Invoke((sender as ButtonContextMenu).Command);
        }

        protected void SetControls(params Control[] controls)
        {
            table.Controls.Clear();
            table.Controls.Add(labelTitle, 0, 0);

            for (int i = 0; i < controls.Length; i++)
                table.Controls.Add(controls[i], 0, i + 1);

            table.Controls.Add(buttonBack, 0, controls.Length + 1);
        }

        protected void SetHeights(params float[] heights)
        {
            table.RowStyles.Clear();
            table.RowCount = table.Controls.Count;

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            foreach (var h in heights)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, h));

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control ctrl in table.Controls)
                {
                    if (ctrl is ButtonContextMenu)
                        (ctrl as ButtonContextMenu).Click -= OnButtonClick;

                    if (ctrl is IDisposable)
                        (ctrl as IDisposable).Dispose();
                }

                table.Controls.Clear();
                table.Dispose();

                BackgroundImage.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
