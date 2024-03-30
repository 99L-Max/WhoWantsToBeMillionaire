namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        public MenuStatistics(int width, int height, string data) : base("Статистика", width, height, 0.05f * height)
        {
            LabelMenu label = new LabelMenu(0.045f * Height);
            label.Text = data;

            SetControls(label);
            SetHeights(5f);
        }
    }
}
