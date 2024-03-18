using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class MenuSettings : ContextMenu, IDisposable
    {
        private readonly Dictionary<GameSettings, float> settings;
        private readonly List<GameComboBox> comboBoxes = new List<GameComboBox>();
        private readonly TableLayoutPanel table;
        private readonly ButtonContextMenu buttonSave;

        public Dictionary<GameSettings, float> SettingsData => settings.ToDictionary(k => k.Key, v => v.Value);

        public MenuSettings(int width, int height, GameSettingsData data) : base("Настройки", width, height, 0.035f * height)
        {
            var keys = Enum.GetValues(typeof(GameSettings)).Cast<GameSettings>();
            settings = keys.ToDictionary(k => k, v => data.GetSettings(v));

            table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

            table.RowCount = table.Controls.Count;

            float fontSize = 0.035f * height;
            var dict = ResourceManager.GetDictionary("Settings.json");

            using (Stream stream = ResourceManager.GetStream("SettingsValues.json", TypeResource.SettingsValues))
            using (StreamReader reader = new StreamReader(stream))
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(reader.ReadToEnd());
                int i = 0;

                table.RowCount = keys.Count();

                foreach (var key in keys)
                {
                    LabelMenu label = new LabelMenu(fontSize);
                    GameComboBox comboBox = new GameComboBox(values[key.ToString()], fontSize);

                    comboBox.SelectedIndexChanged += UpdateSetting;

                    label.Text = dict[key.ToString()];
                    comboBox.LoopedSwitch = key != GameSettings.Volume;
                    comboBox.Tag = key;
                    comboBox.SelectedIndex = (int)settings[key];

                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                    table.Controls.Add(label, 0, i);
                    table.Controls.Add(comboBox, 1, i);

                    comboBoxes.Add(comboBox);
                    i++;
                }
            }

            buttonSave = new ButtonContextMenu(ContextMenuCommand.ApplySettings, fontSize);
            buttonSave.Text = "Применить";
            buttonSave.Click += OnButtonClick;

            SetControls(table, buttonSave);
            SetHeights(table.RowCount, 1f);
        }

        private void UpdateSetting(object sender, EventArgs e)
        {
            var comboBox = sender as GameComboBox;
            var key = (GameSettings)comboBox.Tag;
            settings[key] = comboBox.SelectedIndex;

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
                comboBoxes.ForEach(c => c.SelectedIndexChanged -= UpdateSetting);

                foreach (Control ctrl in table.Controls)
                    if (ctrl is IDisposable)
                        (ctrl as IDisposable).Dispose();

                table.Controls.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
