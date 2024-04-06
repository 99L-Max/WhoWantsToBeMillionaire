using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

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
        private readonly LabelMenu _labelDescriptionMode;
        private readonly GameComboBox _comboBox;
        private readonly ButtonContextMenu _buttonStart;
        private readonly Dictionary<Mode, string> _descriptions;

        public Mode SelectedMode { private set; get; }

        public MenuMode(int width, int height) : base("Выберите режим", width, height, 0.05f * height)
        {
            var modes = JsonManager.GetDictionary<Mode, string>(Resources.Dictionary_Modes).ToDictionary(k => (float)k.Key, v => v.Value);
            var fontSize = 0.05f * Height;

            _descriptions = JsonManager.GetDictionary<Mode, string>(Resources.Dictionary_DescriptionModes);
            _labelDescriptionMode = new LabelMenu(fontSize);
            _comboBox = new GameComboBox(modes, fontSize);
            _buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame);

            _buttonStart.Text = "Старт";

            _comboBox.BackgroundImageLayout = ImageLayout.Stretch;
            _comboBox.BackgroundImage = Resources.ComboBox;

            _buttonStart.Click += OnButtonClick;
            _comboBox.SelectedIndexChanged += ModeChanged;

            SetControls(_comboBox, _labelDescriptionMode, _buttonStart);
            SetHeights(1f, 3f, 1f);

            _comboBox.SelectedIndex = 0;
        }

        private void ModeChanged(object sender, EventArgs e)
        {
            SelectedMode = (Mode)_comboBox.SelectedValue;
            _labelDescriptionMode.Text = _descriptions[SelectedMode];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _comboBox.SelectedIndexChanged -= ModeChanged;

            base.Dispose(disposing);
        }
    }
}
