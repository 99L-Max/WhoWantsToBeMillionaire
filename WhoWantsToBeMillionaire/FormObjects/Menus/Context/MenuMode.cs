using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum Mode
    {
        Classic,
        Amateur,
        Advanced
    }

    class MenuMode : ContextMenu, IDisposable
    {
        private readonly LabelMenu labelDescriptionMode;
        private readonly GameComboBox comboBox;
        private readonly ButtonContextMenu buttonStart;
        private readonly Dictionary<string, string> descriptions;

        public Mode SelectedMode { private set; get; }

        public MenuMode(int width, int height) : base("Выберите режим", width, height, 0.04f * height)
        {
            descriptions = ResourceManager.GetDictionary("DescriptionModes.json");
            var modes = ResourceManager.GetDictionary("Modes.json");

            float fontSize = 0.04f * Height;

            labelDescriptionMode = new LabelMenu(fontSize);
            comboBox = new GameComboBox(modes.Values.ToArray(), fontSize);
            buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame, fontSize);

            buttonStart.Text = "Старт";

            comboBox.BackgroundImageLayout = ImageLayout.Stretch;
            comboBox.BackgroundImage = ResourceManager.GetImage("ComboBox.png");

            buttonStart.Click += OnButtonClick;
            comboBox.SelectedIndexChanged += ModeChanged;

            SetControls(comboBox, labelDescriptionMode, buttonStart);
            SetHeights(1f, 3f, 1f);

            comboBox.SelectedIndex = 0;
        }

        private void ModeChanged(object sender, EventArgs e)
        {
            SelectedMode = (Mode)comboBox.SelectedIndex;
            labelDescriptionMode.Text = descriptions[SelectedMode.ToString()];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                comboBox.SelectedIndexChanged -= ModeChanged;
            }

            base.Dispose(disposing);
        }
    }
}
