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
        private readonly Dictionary<Modes, string> _descriptionModes;

        public MenuMode(float fractionScreenHeight, int widthFraction, int heightFraction) :
            base("Выберите режим", fractionScreenHeight, widthFraction, heightFraction)
        {
            var modesData = JsonReader.GetDictionary<Modes, ModeData>(Resources.Dictionary_ModeData);
            var boxItems = modesData.ToDictionary(mode => Convert.ToSingle(mode.Key), mode => mode.Value.Name);
            var fontSizeItems = 0.05f * Height;

            _descriptionModes = modesData.ToDictionary(pair => pair.Key, pair => pair.Value.Description);
            _labelDescriptionMode = new LabelMenu(fontSizeItems);
            _comboBoxMode = new GameComboBox(boxItems, fontSizeItems);
            _buttonStart = new ButtonContextMenu(ContextMenuCommands.StartGame);

            _buttonStart.Text = "Старт";

            _buttonStart.Click += OnButtonClick;
            _comboBoxMode.SelectedIndexChanged += OnSelectedIndexChanged;

            SetControls(_comboBoxMode, _labelDescriptionMode, _buttonStart);
            SetHeights(1, 3, 1);

            _comboBoxMode.SelectedIndex = 0;
        }

        public Modes SelectedMode { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _comboBoxMode.SelectedIndexChanged -= OnSelectedIndexChanged;

            base.Dispose(disposing);
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMode = (Modes)_comboBoxMode.SelectedValue;
            _labelDescriptionMode.Text = _descriptionModes[SelectedMode];
        }
    }
}
