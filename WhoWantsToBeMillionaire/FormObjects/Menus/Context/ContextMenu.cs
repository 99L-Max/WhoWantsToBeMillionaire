using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum ContextMenuCommand
    {
        Back,
        StartGame,
        ApplySettings
    }

    abstract class ContextMenu : PictureBox, IDisposable
    {
        private readonly LabelMenu labelTitle;
        private readonly ButtonContextMenu buttonBack;
        private readonly TableLayoutPanel table;

        public delegate void EventButtonClick(ContextMenuCommand command);
        public event EventButtonClick ButtonClick;

        public ContextMenu(string title, int width, int height, float fontSize)
        {
            Size = new Size(width, height);
            Location = new Point((MainForm.ScreenRectangle.Width - Width) >> 1, (MainForm.ScreenRectangle.Height - Height) >> 1);
            BackColor = Color.Transparent;

            var border = 12;
            var background = new Bitmap(width, height);
            var rectFrame = new Rectangle(0, 0, Size.Width, Size.Height);
            var rectFill = new Rectangle(border, border, rectFrame.Width - (border << 1), rectFrame.Height - (border << 1));

            using (var g = Graphics.FromImage(background))
            using (var brushFrame = new LinearGradientBrush(rectFrame, Color.Gainsboro, Color.SlateGray, 45f))
            using (var brushFill = new LinearGradientBrush(rectFill, Color.Navy, Color.Black, 90f))
            {
                g.FillRectangle(brushFrame, rectFrame);
                g.FillRectangle(brushFill, rectFill);

                BackgroundImageLayout = ImageLayout.Stretch;
                BackgroundImage = background;
            }

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

        protected void OnButtonClick(object sender, EventArgs e) =>
            ButtonClick.Invoke((sender as ButtonContextMenu).Command);

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

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            foreach (var h in heights)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, h));

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            foreach (var b in table.Controls.OfType<ButtonContextMenu>())
            {
                b.Dock = DockStyle.Fill;
                b.Dock = DockStyle.None;
                b.Anchor = AnchorStyles.None;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control ctrl in table.Controls)
                {
                    if (ctrl is ButtonContextMenu)
                        (ctrl as ButtonContextMenu).Click -= OnButtonClick;

                    ctrl.Dispose();
                }

                table.Controls.Clear();
                table.Dispose();

                BackgroundImage.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
