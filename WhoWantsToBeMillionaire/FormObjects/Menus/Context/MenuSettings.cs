namespace WhoWantsToBeMillionaire
{
    class MenuSettings : ContextMenu
    {
        private readonly GameSettingsData gameSettings;
        private readonly GameTrackBar tbVolume;
        private readonly GameCheckBox chbScreensaver;
        private readonly GameCheckBox chbOptionsSequentially;

        public MenuSettings(int width, int height, GameSettingsData data) : base(width, height, "Настройки", 0.04f * height)
        {
            float fontSize = 0.04f * height;

            gameSettings = data;

            tbVolume = new GameTrackBar();
            chbScreensaver = new GameCheckBox(fontSize);
            chbOptionsSequentially = new GameCheckBox(fontSize);

            chbScreensaver.Text = "Показывать заставку";
            chbOptionsSequentially.Text = "Последовательный показ вариантов";

            SetControls(tbVolume, chbScreensaver, chbOptionsSequentially);
            SetHeights(1f, 1f, 1f);
        }
    }
}
