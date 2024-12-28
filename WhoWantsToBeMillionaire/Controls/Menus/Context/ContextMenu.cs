using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ContextMenuCommand { Back, StartGame, ApplySettings, Exit }

    abstract class ContextMenu : PictureBox, IDisposable
    {
        private readonly LabelMenu _labelTitle;
        private readonly ButtonContextMenu _buttonBack;
        private readonly TableLayoutPanel _table;

        public Action<ContextMenuCommand> ButtonClick;

        public ContextMenu(string title, int width, int height, float fontSize)
        {
            Size = new Size(width, height);
            Location = new Point(GameConst.ScreenSize.Width - Width >> 1, GameConst.ScreenSize.Height - Height >> 1);
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Stretch;
            BackgroundImage = Painter.CreateFilledPanel(Size, 12, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);

            _table = new TableLayoutPanel();
            _labelTitle = new LabelMenu(fontSize, ContentAlignment.MiddleCenter);
            _buttonBack = new ButtonContextMenu(ContextMenuCommand.Back);

            _labelTitle.Text = title;
            _buttonBack.Text = "Назад";

            _buttonBack.Click += OnButtonClick;

            _table.Size = new Size((int)(0.9f * Width), (int)(0.9f * Height));
            _table.Location = new Point(Width - _table.Width >> 1, Height - _table.Height >> 1);

            Controls.Add(_table);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control ctrl in _table.Controls)
                {
                    if (ctrl is ButtonContextMenu btn)
                        btn.Click -= OnButtonClick;

                    ctrl.Dispose();
                }

                _table.Controls.Clear();
                _table.Dispose();

                BackgroundImage.Dispose();
            }

            base.Dispose(disposing);
        }

        protected void OnButtonClick(object sender, EventArgs e)
        {
            if (sender is ButtonContextMenu btn)
                ButtonClick?.Invoke(btn.Command);
        }

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
                if (ctrl is ButtonContextMenu btn)
                    btn.AlignSize(6f, 1f);
        }
    }
}
