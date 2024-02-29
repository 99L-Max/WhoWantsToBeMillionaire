using System;
using System.Drawing;

namespace WhoWantsToBeMillionaire
{
    class MenuStatistics : ContextMenu, IDisposable
    {
        private readonly StatisticsData statistics;
        private readonly LabelMenu labelTitle;
        private readonly LabelMenu labelData;
        private readonly ButtonContextMenu buttonReset;
        private readonly ButtonContextMenu buttonBack;

        public MenuStatistics(int width, int height, StatisticsData data) : base(width, height)
        {
            statistics = data;

            labelTitle = new LabelMenu(0.05f * Height, ContentAlignment.MiddleCenter);
            labelData = new LabelMenu(0.03f * Height);

            labelTitle.Text = "Статистика";
            labelData.Text = statistics.ToString();

            float fontSize = 0.04f * Height;

            buttonReset = new ButtonContextMenu(ContextMenuCommand.ResetStatistics, fontSize);
            buttonBack = new ButtonContextMenu(ContextMenuCommand.Back, fontSize);

            buttonReset.Text = "Сбросить";
            buttonBack.Text = "Назад";

            table.Controls.Add(labelTitle, 0, 0);
            table.Controls.Add(labelData, 0, 1);
            table.Controls.Add(buttonReset, 0, 2);
            table.Controls.Add(buttonBack, 0, 3);

            buttonReset.Click += OnButtonClick;
            buttonBack.Click += OnButtonClick;

            SetHeights(new float[] { 1f, 4f, 1f, 1f });
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                buttonReset.Click -= OnButtonClick;
                buttonBack.Click -= OnButtonClick;
            }

            base.Dispose(disposing);
        }
    }
}
