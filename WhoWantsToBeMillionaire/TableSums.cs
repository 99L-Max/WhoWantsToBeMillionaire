using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class TableSums : MovingPictureBox
    {
        private bool taskCanceled;
        private int numberNextSum;

        private readonly TableLayoutPanel table;
        private readonly RowTableSums[] rowsSum;

        public delegate void EventSaveSumSelected(int sum);
        public event EventSaveSumSelected SaveSumSelected;

        public int Prize { private set; get; }

        public string TextPrize { private set; get; }

        public int NumberNextSum
        {
            set
            {
                numberNextSum = value;
                SetSelectedSum(numberNextSum - 1);
            }
            get => numberNextSum;
        }

        public int MaxNumberSum => rowsSum.Length;

        public int[] SaveSums => rowsSum.Where(r => r.IsSaveSum && r.Number != rowsSum.Length).Select(r => r.Sum).ToArray();

        public string NextSum => string.Format("{0:#,0}", rowsSum[numberNextSum - 1].Sum);

        public TableSums(int width, int height) : base(width, height)
        {
            using (Stream stream = ResourceProcessing.GetStream("Sums.json", TypeResource.Sums))
            using (StreamReader reader = new StreamReader(stream))
            {
                int[] sums = JsonConvert.DeserializeObject<int[]>(reader.ReadToEnd());

                BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Background_Amounts.png"), width, height);

                table = new TableLayoutPanel();
                rowsSum = new RowTableSums[sums.Length];

                int heightRow = (int)(height * 0.67f / sums.Length);

                table.Size = new Size((int)(0.8f * width), heightRow * sums.Length + 1);
                table.Location = new Point((Width - table.Width) / 2, (int)(0.2f * height));
                table.BackColor = Color.Transparent;

                table.RowCount = sums.Length;

                for (int i = 0; i < table.RowCount; i++)
                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / table.RowCount));

                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                for (int i = sums.Length - 1; i > -1; i--)
                {
                    rowsSum[i] = new RowTableSums(i + 1, sums[i]);
                    table.Controls.Add(rowsSum[i], 0, sums.Length - rowsSum[i].Number);
                }

                Controls.Add(table);
            }
        }

        public void Reset(Mode mode)
        {
            X = MainForm.RectScreen.Width;

            table.Visible = false;

            foreach (var row in rowsSum)
                row.Reset();

            if (mode == Mode.Classic)
                foreach (var row in rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            SetPrize(0);
        }

        public async new Task Show()
        {
            await MoveX(MainForm.RectScreen.Width - Width, 600 / MainForm.DeltaTime);

            table.Visible = true;
        }

        private void SetPrize(int numberSum)
        {
            try
            {
                Prize = rowsSum[numberSum - 1].Sum;
                TextPrize = numberNextSum < rowsSum.Length ? string.Format("{0:#,0}", Prize) : "МИЛЛИОНЕР!";
            }
            catch (IndexOutOfRangeException)
            {
                Prize = 0;
                TextPrize = "0";
            }
        }

        private void SetSelectedSum(int number)
        {
            foreach (var row in rowsSum)
            {
                row.IsSelected = row.Number == number;
                row.IconVisible = row.Number <= number;
            }
        }

        public void AddSelectionSaveSum()
        {
            foreach (var row in rowsSum)
            {
                row.Reset();
                row.AddMouseEvents();
                row.Click += SelectSaveSum;
            }
        }

        private void SelectSaveSum(object sender, EventArgs e)
        {
            RowTableSums saveSum = sender as RowTableSums;

            foreach (var row in rowsSum)
            {
                row.Click -= SelectSaveSum;
                row.RemoveMouseEvents();
                row.IconVisible = row.Number <= saveSum.Number;
            }

            saveSum.IsSaveSum = true;
            saveSum.IsSelected = true;

            rowsSum[rowsSum.Length - 1].IsSaveSum = true;

            SaveSumSelected.Invoke(saveSum.Sum);
        }

        public async Task ShowSums()
        {
            taskCanceled = false;

            Queue<RowTableSums> rows = new Queue<RowTableSums>(rowsSum);
            RowTableSums row = null;

            while (rows.Count > 0)
            {
                rows.Peek().IsSelected = true;

                if (row != null)
                {
                    row.IsSelected = false;
                    row.IconVisible = true;
                }

                row = rows.Dequeue();

                await Task.Delay(250);

                if(taskCanceled)
                {
                    IncompleteResetRows();
                    return;
                }
            }

            rowsSum[rowsSum.Length - 1].IconVisible = true;
        }

        public async Task ShowSaveSums()
        {
            taskCanceled = false;

            IncompleteResetRows();

            foreach (var row in rowsSum)
            {
                row.IconVisible = true;

                if (row.IsSaveSum)
                {
                    row.IsSelected = true;

                    await Task.Delay(1000);

                    row.IsSelected = false;
                }

                if (taskCanceled)
                {
                    IncompleteResetRows();
                    return;
                }
            }

            rowsSum[rowsSum.Length - 1].IsSelected = true;
        }

        private void IncompleteResetRows()
        {
            foreach (var row in rowsSum)
                row.IsSelected = row.IconVisible = false;
        }

        public void CancelTask() => taskCanceled = true;

        public void UpdatePrize(bool isCorrect)
        {
            if (isCorrect)
            {
                SetPrize(numberNextSum);
                NumberNextSum++;
            }
            else
            {
                numberNextSum = rowsSum.Where(r => r.IsSaveSum && r.Number < numberNextSum).Select(r => r.Number).DefaultIfEmpty(0).Last();
                SetSelectedSum(numberNextSum);
                SetPrize(numberNextSum - 1);
            }
        }
    }
}