using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MenuSettings : ContextMenu, IDisposable
    {
        private readonly Dictionary<GameSettings, float> _settings;
        private readonly List<GameComboBox> _comboBoxes = new List<GameComboBox>();
        private readonly TableLayoutPanel _table;
        private readonly ButtonContextMenu _buttonSave;

        public MenuSettings(float fractionScreenHeight, int widthFraction, int heightFraction, SettingsData data)
            : base("Настройки", fractionScreenHeight, widthFraction, heightFraction)
        {
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();
            var fontSizeItems = 0.04f * Height;
            var dictLabelText = JsonManager.GetDictionary<GameSettings, string>(Resources.Dictionary_Settings);
            var values = JsonManager.GetDictionary<GameSettings, Dictionary<float, string>>(Resources.Settings_Values);
            var i = 0;

            _settings = keys.ToDictionary(k => k, v => data.GetSettings(v));

            _table = new TableLayoutPanel();
            _table.RowCount = keys.Count();
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 4f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1f));

            _buttonSave = new ButtonContextMenu(ContextMenuCommand.ApplySettings);
            _buttonSave.Text = "Применить";
            _buttonSave.Click += OnButtonClick;

            foreach (var key in keys)
            {
                var label = new LabelMenu(fontSizeItems);
                var comboBox = new GameComboBox(values[key], fontSizeItems);

                label.Text = dictLabelText[key];
                label.Dock = DockStyle.Fill;

                comboBox.Looped = key != GameSettings.Volume;
                comboBox.Tag = key;
                comboBox.SelectedValue = _settings[key];
                comboBox.SelectedIndexChanged += OnGameComboBoxValueChanged;
                comboBox.Dock = DockStyle.Fill;

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                _table.Controls.Add(label, 0, i);
                _table.Controls.Add(comboBox, 1, i);

                _comboBoxes.Add(comboBox);

                i++;
            }

            SetControls(_table, _buttonSave);
            SetHeights(_table.RowCount, 1);

            foreach (Control ctrl in _table.Controls)
                if (ctrl is IAlignSize a)
                    a.AlignSize();
        }

        public Dictionary<GameSettings, float> SettingsData =>
            _settings.ToDictionary(k => k.Key, v => v.Value);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _comboBoxes.ForEach(c => c.SelectedIndexChanged -= OnGameComboBoxValueChanged);

                foreach (Control ctrl in _table.Controls)
                    ctrl.Dispose();

                _table.Controls.Clear();
            }

            base.Dispose(disposing);
        }

        private void OnGameComboBoxValueChanged(object sender, EventArgs e)
        {
            if (sender is GameComboBox comboBox)
            {
                var key = (GameSettings)comboBox.Tag;

                _settings[key] = comboBox.SelectedValue;

                if (key == GameSettings.Volume)
                {
                    GameSound.SetVolume(comboBox.SelectedValue);
                    GameMusic.SetVolume(comboBox.SelectedValue);
                }
            }
        }
    }
}
