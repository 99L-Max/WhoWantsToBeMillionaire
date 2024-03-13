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
        private readonly LabelMenu labelDescriptionMode;
        private readonly GameComboBox comboBox;
        private readonly ButtonContextMenu buttonStart;

        public Mode SelectedMode { private set; get; }

        public MenuMode(int width, int height) : base(width, height, "Выберите режим", 0.04f * height)
        {
            var modes = ResourceManager.GetDictionary("Modes.json");

            float fontSize = 0.04f * Height;

            labelDescriptionMode = new LabelMenu(fontSize);
            comboBox = new GameComboBox(modes.Values.ToArray(), fontSize);
            buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame, fontSize);

            buttonStart.Text = "Старт";

            buttonStart.Click += OnButtonClick;
            comboBox.SelectedIndexChanged += ModeChanged;

            SetControls(comboBox, labelDescriptionMode, buttonStart);
            SetHeights(1f, 3f, 1f);

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
