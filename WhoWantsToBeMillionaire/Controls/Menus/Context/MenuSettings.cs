﻿using System;
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
            var fontSize = 0.04f * height;
            var labels = JsonManager.GetDictionary<GameSettings, string>(Resources.Dictionary_Settings);
            var values = JsonManager.GetDictionary<GameSettings, Dictionary<float, string>>(Resources.Settings_Values);
            var i = 0;

            _settings = keys.ToDictionary(k => k, v => data.GetSettings(v));

            _table = new TableLayoutPanel();
            _table.Dock = DockStyle.Fill;
            _table.RowCount = keys.Count();
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

            _buttonSave = new ButtonContextMenu(ContextMenuCommand.ApplySettings);
            _buttonSave.Text = "Применить";
            _buttonSave.Click += OnButtonClick;

            foreach (var key in keys)
            {
                var label = new LabelMenu(fontSize);
                var comboBox = new GameComboBox(values[key], fontSize);

                label.Text = labels[key];

                comboBox.Looped = key != GameSettings.Volume;
                comboBox.Tag = key;
                comboBox.SelectedValue = _settings[key];
                comboBox.SelectedIndexChanged += OnGameComboBoxValueChanged;

                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                _table.Controls.Add(label, 0, i);
                _table.Controls.Add(comboBox, 1, i);

                _comboBoxes.Add(comboBox);

                i++;
            }

            SetControls(_table, _buttonSave);
            SetHeights(_table.RowCount, 1f);
        }

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
                    Sound.SetVolume(comboBox.SelectedValue);
            }
        }
    }
}
