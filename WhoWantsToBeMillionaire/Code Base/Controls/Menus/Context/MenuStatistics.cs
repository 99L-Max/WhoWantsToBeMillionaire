namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        public MenuStatistics(float fractionScreenHeight, int widthFraction, int heightFraction, string data) :
            base("Статистика", fractionScreenHeight, widthFraction, heightFraction)
        {
            var label = new LabelMenu(0.045f * Height) { Text = data };

            SetControls(label);
            SetHeights(5);
        }
    }
}
