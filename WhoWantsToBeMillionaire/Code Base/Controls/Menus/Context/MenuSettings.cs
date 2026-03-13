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
            var settings = JsonReader.GetDictionary<GameSettings, GameSettingsValues>(Resources.Dictionary_Settings);
            var fontSizeItems = 0.04f * Height;
            var i = 0;

            _settings = settings.ToDictionary(pair => pair.Key, pair => data.GetSettings(pair.Key));

            _table = new TableLayoutPanel();
            _table.RowCount = settings.Count();
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 4f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1f));

            _buttonSave = new ButtonContextMenu(ContextMenuCommands.ApplySettings);
            _buttonSave.Text = "Применить";
            _buttonSave.Click += OnButtonClick;

            foreach (var key in settings.Keys)
            {
                var label = new LabelMenu(fontSizeItems)
                {
                    Text = settings[key].Name,
                    Dock = DockStyle.Fill
                };

                var comboBox = new GameComboBox(settings[key].ComboBoxItems, fontSizeItems)
                {
                    Looped = key != GameSettings.Volume,
                    Tag = key,
                    SelectedValue = _settings[key],
                    Dock = DockStyle.Fill
                };

                comboBox.SelectedIndexChanged += OnGameComboBoxValueChanged;

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                _table.Controls.Add(label, 0, i);
                _table.Controls.Add(comboBox, 1, i);

                _comboBoxes.Add(comboBox);

                i++;
            }

            SetControls(_table, _buttonSave);
            SetHeights(_table.RowCount, 1);

            foreach (Control ctrl in _table.Controls)
                if (ctrl is IResizable a)
                    a.AlignSize();
        }

        public Dictionary<GameSettings, float> CopySettingsData => new Dictionary<GameSettings, float>(_settings);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _comboBoxes.ForEach(box => box.SelectedIndexChanged -= OnGameComboBoxValueChanged);

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
                    Sound.SetVolume(comboBox.SelectedValue);
                    Music.SetVolume(comboBox.SelectedValue);
                }
            }
        }
    }
}
