using System;
using System.Collections.Generic;
using System.Linq;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuMode : ContextMenu, IDisposable
    {
        private readonly LabelMenu _labelDescriptionMode;
        private readonly GameComboBox _comboBoxMode;
        private readonly ButtonContextMenu _buttonStart;
        private readonly Dictionary<Mode, string> _descriptionModes;

        public MenuMode(float fractionScreenHeight, int widthFraction, int heightFraction) :
            base("Выберите режим", fractionScreenHeight, widthFraction, heightFraction)
        {
            var modes = JsonManager.GetDictionary<Mode, string>(Resources.Dictionary_Modes).ToDictionary(k => (float)k.Key, v => v.Value);
            var fontSizeItems = 0.05f * Height;

            _descriptionModes = JsonManager.GetDictionary<Mode, string>(Resources.Dictionary_DescriptionModes);
            _labelDescriptionMode = new LabelMenu(fontSizeItems);
            _comboBoxMode = new GameComboBox(modes, fontSizeItems);
            _buttonStart = new ButtonContextMenu(ContextMenuCommand.StartGame);

            _buttonStart.Text = "Старт";

            _buttonStart.Click += OnButtonClick;
            _comboBoxMode.SelectedIndexChanged += ModeChanged;

            SetControls(_comboBoxMode, _labelDescriptionMode, _buttonStart);
            SetHeights(1, 3, 1);

            _comboBoxMode.SelectedIndex = 0;
        }

        public Mode SelectedMode { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _comboBoxMode.SelectedIndexChanged -= ModeChanged;

            base.Dispose(disposing);
        }

        private void ModeChanged(object sender, EventArgs e)
        {
            SelectedMode = (Mode)_comboBoxMode.SelectedValue;
            _labelDescriptionMode.Text = _descriptionModes[SelectedMode];
        }
    }
}
