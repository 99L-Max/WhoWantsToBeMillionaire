using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ContextMenuCommand
    {
        Back,
        StartGame,
        ApplySettings,
        Exit
    }

    abstract class ContextMenu : PictureBox, IDisposable
    {
        private readonly LabelMenu _labelTitle;
        private readonly ButtonContextMenu _buttonBack;
        private readonly TableLayoutPanel _table;

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

            _table = new TableLayoutPanel();
            _labelTitle = new LabelMenu(fontSize, ContentAlignment.MiddleCenter);
            _buttonBack = new ButtonContextMenu(ContextMenuCommand.Back);

            _labelTitle.Text = title;
            _buttonBack.Text = "Назад";

            _buttonBack.Click += OnButtonClick;

            _table.Size = new Size((int)(0.9f * Width), (int)(0.9f * Height));
            _table.Location = new Point((Width - _table.Width) >> 1, (Height - _table.Height) >> 1);

            Controls.Add(_table);
        }

        protected void OnButtonClick(object sender, EventArgs e) =>
            ButtonClick.Invoke((sender as ButtonContextMenu).Command);

        protected void SetControls(params Control[] controls)
        {
            _table.Controls.Clear();

            _table.Controls.Add(_labelTitle, 0, 0);

            for (int i = 0; i < controls.Length; i++)
                _table.Controls.Add(controls[i], 0, i + 1);

            _table.Controls.Add(_buttonBack, 0, controls.Length + 1);
        }

        protected void SetHeights(params float[] heights)
        {
            _table.RowStyles.Clear();

            _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));

            foreach (var h in heights)
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, h));

            _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));

            foreach (var ctrl in _table.Controls)
                if (ctrl is ButtonContextMenu)
                    (ctrl as ButtonContextMenu).AlignSize(6f, 1f);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control ctrl in _table.Controls)
                {
                    if (ctrl is ButtonContextMenu)
                        (ctrl as ButtonContextMenu).Click -= OnButtonClick;

                    ctrl.Dispose();
                }

                _table.Controls.Clear();
                _table.Dispose();

                BackgroundImage.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
