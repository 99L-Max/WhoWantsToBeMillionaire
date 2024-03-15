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
    class TableSums : MovingPictureBox, IReset
    {
        private bool taskCanceled;
        private int numberQuestion;

        private readonly TableLayoutPanel table;
        private readonly RowTableSums[] rowsSum;

        public delegate void EventSaveSumSelected(int sum);
        public event EventSaveSumSelected SaveSumSelected;

        public int Prize { private set; get; }

        public string TextPrize { private set; get; }

        public string NextSum => string.Format("{0:#,0}", rowsSum[Math.Min(numberQuestion - 1, rowsSum.Length - 1)].Sum);

        public int[] SaveSums => rowsSum.Where(r => r.IsSaveSum && r.Number < Question.MaxNumber).Select(r => r.Sum).ToArray();

        public bool NowSaveSum => rowsSum[numberQuestion - 1].IsSaveSum;

        public TableSums(int width, int height) : base(width, height)
        {
            using (Stream stream = ResourceManager.GetStream("Sums.json", TypeResource.Sums))
            using (StreamReader reader = new StreamReader(stream))
            {
                int[] sums = JsonConvert.DeserializeObject<int[]>(reader.ReadToEnd());

                BackgroundImage = new Bitmap(ResourceManager.GetImage("Background_Sums.png"), width, height);

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

        public void Reset(Mode mode = Mode.Classic)
        {
            X = MainForm.RectScreen.Width;

            table.Visible = false;

            foreach (var row in rowsSum)
                row.Reset();

            if (mode == Mode.Classic)
                foreach (var row in rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            numberQuestion = 1;
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
                TextPrize = numberSum < rowsSum.Length ? string.Format("{0:#,0}", Prize) : "МИЛЛИОНЕР!";
            }
            catch (IndexOutOfRangeException)
            {
                Prize = 0;
                TextPrize = "0";
            }
        }

        public void SetSelectedSum(int number)
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

            SetSelectedSum(saveSum.Number);

            foreach (var row in rowsSum)
            {
                row.Click -= SelectSaveSum;
                row.RemoveMouseEvents();
            }

            rowsSum[rowsSum.Length - 1].IsSaveSum = saveSum.IsSaveSum = true;

            Sound.Play("SavaSumSelected.wav");
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

                if (taskCanceled)
                {
                    Clear();
                    return;
                }
            }

            rowsSum[rowsSum.Length - 1].IconVisible = true;
        }

        public async Task ShowSaveSums()
        {
            taskCanceled = false;

            Clear();

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
                    Clear();
                    return;
                }
            }

            rowsSum[rowsSum.Length - 1].IsSelected = true;
        }

        public void CancelTask() => taskCanceled = true;

        public void Update(bool isCorrectAnswer)
        {
            if (isCorrectAnswer)
            {
                SetPrize(numberQuestion);
                SetSelectedSum(numberQuestion++);
            }
            else
            {
                int number = rowsSum.Where(r => r.IsSaveSum && r.Number < numberQuestion).Select(r => r.Number).DefaultIfEmpty(0).Last();
                SetSelectedSum(number);
                SetPrize(number);
            }
        }

        public void Clear()
        {
            foreach (var row in rowsSum)
                row.IsSelected = row.IconVisible = false;
        }
    }
}