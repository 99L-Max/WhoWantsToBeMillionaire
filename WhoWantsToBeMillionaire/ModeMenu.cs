using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum Mode
    {
        Classic,
        Amateur,
        Advanced,
    }

    class ModeMenu : ContextMenu
    {
        private readonly Label labelMode;
        private readonly Label labelDescriptionMode;
        private readonly GameComboBox comboBox;
        private readonly ButtonContextMenu buttonStart;
        private readonly ButtonContextMenu buttonBack;

        public Mode SelectedMode { private set; get; }

        public ModeMenu(Size size) : base(size)
        {
            string[] modes = { "Классический", "Любительский", "Расширенный" };
            float[] widths = new float[] { 1f, 1f, 3f, 1f, 1f };

            table.RowCount = widths.Length;

            foreach (var w in widths)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, w));

            float fontSize = 0.04f * size.Height;
            Size sizeButtonCell = new Size(Width, (int)(Height / widths.Sum()));

            labelMode = new Label();
            labelDescriptionMode = new Label();

            labelDescriptionMode.Dock = labelMode.Dock = DockStyle.Fill;
            labelDescriptionMode.ForeColor = labelMode.ForeColor = Color.White;

            labelMode.TextAlign = ContentAlignment.MiddleCenter;

            labelMode.Font = new Font("", 1.2f * fontSize);
            labelDescriptionMode.Font = new Font("", fontSize);

            labelMode.Text = "Выберите режим";

            comboBox = new GameComboBox(modes, fontSize);
            comboBox.SelectedIndexChanged += ModeChanged;

            buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame, "Старт", fontSize);
            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, "Назад", fontSize);

            buttonStart.Click += OnButtonClick;
            buttonBack.Click += OnButtonClick;

            table.Controls.Add(labelMode, 0, 0);
            table.Controls.Add(comboBox, 0, 1);
            table.Controls.Add(labelDescriptionMode, 0, 2);
            table.Controls.Add(buttonStart, 0, 3);
            table.Controls.Add(buttonBack, 0, 4);

            comboBox.SelectedIndex = 0;
        }

        private void ModeChanged()
        {
            SelectedMode = (Mode)comboBox.SelectedIndex;

            switch (SelectedMode)
            {
                case Mode.Classic:
                    labelDescriptionMode.Text = "\nПодсказок: 3\n\nНесгораемых сумм: 2";
                    break;

                case Mode.Amateur:
                    labelDescriptionMode.Text = "\nПодсказок: 4\n\nНесгораемых сумм: 1";
                    break;

                case Mode.Advanced:
                    labelDescriptionMode.Text = "\nПодсказок: 5\n\nНесгораемых сумм: 1";
                    break;

                default:
                    labelDescriptionMode.Text = string.Empty;
                    break;
            }
        }
    }
}
