using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    abstract class ContextMenu : GameContol, IDisposable
    {
        private readonly LabelMenu _labelTitle;
        private readonly ButtonContextMenu _buttonBack;
        private readonly TableLayoutPanel _table;

        public Action<ContextMenuCommand> ButtonClick;

        public ContextMenu(string title, float fractionScreenHeight, int widthFraction, int heightFraction, float ratioHeightFontSize = 0.05f) :
            base(BasicSize.Height, (int)(GameConst.ScreenSize.Height * fractionScreenHeight), widthFraction, heightFraction)
        {
            Location = new Point(GameConst.ScreenSize.Width - Width >> 1, GameConst.ScreenSize.Height - Height >> 1);
            BackgroundImage = Painter.CreateFilledPanel(Size, 12, Color.Gainsboro, Color.SlateGray, 45f, Color.Navy, Color.Black, 90f);

            _table = new TableLayoutPanel();
            _labelTitle = new LabelMenu(ratioHeightFontSize * Height, ContentAlignment.MiddleCenter);
            _buttonBack = new ButtonContextMenu(ContextMenuCommand.Back);

            _labelTitle.Text = title;
            _buttonBack.Text = "Назад";

            _buttonBack.Click += OnButtonClick;

            _table.Size = Resizer.Resize(Size, 0.9f);
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
            _table.Controls.AddRange(controls);
            _table.Controls.Add(_buttonBack, 0, controls.Length + 1);

            foreach (Control ctrl in _table.Controls)
                ctrl.Dock = DockStyle.Fill;
        }

        protected void SetHeights(params int[] heights)
        {
            _table.RowStyles.Clear();

            _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            foreach (var h in heights)
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, h));

            _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            var rowCount = heights.Sum() + 2;
            _table.Height = _table.Height / rowCount * rowCount + 1;

            foreach (var ctrl in _table.Controls)
                if (ctrl is IAlignSize a)
                    a.AlignSize();
        }
    }
}