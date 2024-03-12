using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu
    {
        private readonly LabelMenu labelTitle;
        private readonly LabelMenu labelData;
        private readonly ButtonContextMenu buttonBack;

        public MenuStatistics(int width, int height, string data) : base(width, height)
        {
            labelTitle = new LabelMenu(0.05f * Height, ContentAlignment.MiddleCenter);
            labelData = new LabelMenu(0.035f * Height);

            labelTitle.Text = "Статистика";
            labelData.Text = data;

            float fontSize = 0.04f * Height;

            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);

            buttonBack.Text = "Назад";

            buttonBack.Click += OnButtonClick;

            table.Controls.Add(labelTitle, 0, 0);
            table.Controls.Add(labelData, 0, 1);
            table.Controls.Add(buttonBack, 0, 2);

            SetHeights(1f, 5f, 1f);
        }
    }
}
