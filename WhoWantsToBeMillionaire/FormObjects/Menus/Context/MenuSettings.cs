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

        public Dictionary<GameSettings, float> SettingsData =>
            _settings.ToDictionary(k => k.Key, v => v.Value);

        public MenuSettings(int width, int height, GameSettingsData data) : base("Настройки", width, height, 0.05f * height)
        {
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();
            _settings = keys.ToDictionary(k => k, v => data.GetSettings(v));

            _table = new TableLayoutPanel();
            _table.Dock = DockStyle.Fill;
            _table.RowCount = keys.Count();

            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

            var fontSize = 0.04f * height;
            var dict = JsonManager.GetDictionary(Resources.Dictionary_Settings);
            var values = JsonManager.GetObject<Dictionary<string, string[]>>(Resources.Dictionary_Settings);
            var i = 0;

            foreach (var key in keys)
            {
                LabelMenu label = new LabelMenu(fontSize);
                GameComboBox comboBox = new GameComboBox(values[key.ToString()], fontSize);

                comboBox.SelectedIndexChanged += UpdateSetting;

                label.Text = dict[key.ToString()];
                comboBox.LoopedSwitch = key != GameSettings.Volume;
                comboBox.Tag = key;
                comboBox.SelectedIndex = (int)_settings[key];

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                _table.Controls.Add(label, 0, i);
                _table.Controls.Add(comboBox, 1, i);

                _comboBoxes.Add(comboBox);
                i++;
            }

            _buttonSave = new ButtonContextMenu(ContextMenuCommand.ApplySettings, 0.05f * height);
            _buttonSave.Text = "Применить";
            _buttonSave.Click += OnButtonClick;

            SetControls(_table, _buttonSave);
            SetHeights(_table.RowCount, 1f);
        }

        private void UpdateSetting(object sender, EventArgs e)
        {
            var comboBox = sender as GameComboBox;
            var key = (GameSettings)comboBox.Tag;
            _settings[key] = comboBox.SelectedIndex;

            if (key == GameSettings.Volume)
            {
                float volume = float.Parse(comboBox.Text) / 100f;
                Sound.SetVolume(volume);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _comboBoxes.ForEach(c => c.SelectedIndexChanged -= UpdateSetting);

                foreach (Control ctrl in _table.Controls)
                    if (ctrl is IDisposable)
                        (ctrl as IDisposable).Dispose();

                _table.Controls.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
