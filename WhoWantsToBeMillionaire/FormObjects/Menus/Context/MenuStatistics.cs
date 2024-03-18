namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        public MenuStatistics(int width, int height, string data) : base("Статистика", width, height, 0.04f * height)
        {
            LabelMenu label = new LabelMenu(0.035f * Height);
            label.Text = data;

            SetControls(label);
            SetHeights(5f);
        }
    }
}
