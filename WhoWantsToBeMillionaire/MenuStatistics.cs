using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        private readonly StatisticsData statistics;
        private readonly LabelMenu labelTitle;
        private readonly LabelMenu labelData;
        private readonly ButtonContextMenu buttonBack;

        public MenuStatistics(int width, int height, StatisticsData data) : base(width, height)
        {
            statistics = data;

            labelTitle = new LabelMenu(0.05f * Height, ContentAlignment.MiddleCenter);
            labelData = new LabelMenu(0.03f * Height);

            labelTitle.Text = "Статистика";
            labelData.Text = statistics.ToString();

            float fontSize = 0.04f * Height;

            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);

            buttonBack.Text = "Назад";

            buttonBack.Click += OnButtonClick;

            table.Controls.Add(labelTitle, 0, 0);
            table.Controls.Add(labelData, 0, 1);
            table.Controls.Add(buttonBack, 0, 2);

            SetHeights(new float[] { 1f, 5f, 1f });
        }
    }
}
