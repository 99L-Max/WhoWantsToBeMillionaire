using System.Drawing;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    enum Mode
    {
        Classic,
        Amateur,
        Advanced,
    }

    class MenuMode : ContextMenu
    {
        private readonly LabelMenu labelTitle;
        private readonly LabelMenu labelDescriptionMode;
        private readonly GameComboBox comboBox;
        private readonly ButtonContextMenu buttonStart;
        private readonly ButtonContextMenu buttonBack;

        public Mode SelectedMode { private set; get; }

        public MenuMode(int width, int height) : base(width, height)
        {
            var modes = ResourceProcessing.GetDictionary("Modes.json");

            float fontSize = 0.04f * Height;

            labelTitle = new LabelMenu(1.2f * fontSize, ContentAlignment.MiddleCenter);
            labelDescriptionMode = new LabelMenu(fontSize);

            labelTitle.Text = "Выберите режим";

            comboBox = new GameComboBox(modes.Values.ToArray(), fontSize);
            comboBox.SelectedIndexChanged += ModeChanged;

            buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame, fontSize);
            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);

            buttonStart.Text = "Старт";
            buttonBack.Text = "Назад";

            buttonStart.Click += OnButtonClick;
            buttonBack.Click += OnButtonClick;

            table.Controls.Add(labelTitle, 0, 0);
            table.Controls.Add(comboBox, 0, 1);
            table.Controls.Add(labelDescriptionMode, 0, 2);
            table.Controls.Add(buttonStart, 0, 3);
            table.Controls.Add(buttonBack, 0, 4);

            SetHeights(new float[] { 1f, 1f, 3f, 1f, 1f });

            comboBox.SelectedIndex = 0;
        }

        private void ModeChanged()
        {
            SelectedMode = (Mode)comboBox.SelectedIndex;

            switch (SelectedMode)
            {
                case Mode.Classic:
                    labelDescriptionMode.Text = "Подсказок: 3\n\nНесгораемых сумм: 2";
                    break;

                case Mode.Amateur:
                    labelDescriptionMode.Text = "Подсказок: 4\n\nНесгораемых сумм: 1";
                    break;

                case Mode.Advanced:
                    labelDescriptionMode.Text = "Подсказок: 5\n\nНесгораемых сумм: 1";
                    break;

                default:
                    labelDescriptionMode.Text = string.Empty;
                    break;
            }
        }
    }
}
