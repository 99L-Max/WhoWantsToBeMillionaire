using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MenuSettings : ContextMenu
    {
        private readonly GameSettingsData gameSettings;
        private readonly LabelMenu labelTitle;
        private readonly ButtonContextMenu buttonBack;
        private readonly GameTrackBar tbVolume;
        private readonly GameCheckBox chbScreensaver;
        private readonly GameCheckBox chbOptionsSequentially;

        public MenuSettings(int width, int height, GameSettingsData data) : base(width, height)
        {
            float fontSize = 0.035f * Height;

            gameSettings = data;

            labelTitle = new LabelMenu(0.05f * Height, ContentAlignment.MiddleCenter);
            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);
            tbVolume = new GameTrackBar();
            chbScreensaver = new GameCheckBox(fontSize);
            chbOptionsSequentially = new GameCheckBox(fontSize);

            labelTitle.Text = "Настройки";
            chbScreensaver.Text = "Показывать заставку";
            chbOptionsSequentially.Text = "Последовательный показ вариантов";
            buttonBack.Text = "Назад";

            buttonBack.Click += OnButtonClick;

            table.Controls.Add(labelTitle, 0, 0);
            table.Controls.Add(tbVolume, 0, 1);
            table.Controls.Add(chbScreensaver, 0, 2);
            table.Controls.Add(chbOptionsSequentially, 0, 3);
            table.Controls.Add(buttonBack, 0, 4);

            SetHeights(1f, 1f, 1f, 1f, 1f);
        }
    }
}
