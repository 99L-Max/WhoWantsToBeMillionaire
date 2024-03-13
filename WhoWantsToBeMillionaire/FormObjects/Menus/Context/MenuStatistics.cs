namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        private readonly LabelMenu labelData;

        public MenuStatistics(int width, int height, string data) : base(width, height, "Статистика", 0.04f * height)
        {
            labelData = new LabelMenu(0.035f * Height);
            labelData.Text = data;

            SetControls(labelData);
            SetHeights(5f);
        }
    }
}
