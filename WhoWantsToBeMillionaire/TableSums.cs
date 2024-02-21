using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class TableSums : MovingPictureBox
    {
        private int numberQuestion;

        private readonly TableLayoutPanel tableSums;
        private readonly RowTableSum[] rowsSum;

        public delegate void EventSaveSumSelected(int sum);
        public event EventSaveSumSelected SaveSumSelected;

        public int Prize { private set; get; }

        public int NumberQuestion
        {
            set
            {
                numberQuestion = value;
                SetSelectedSum(numberQuestion - 1);
            }
            get => numberQuestion;
        }

        public int MaxNumberQuestion => rowsSum.Length;

        public int[] SaveSums => rowsSum.Where(r => r.IsSaveSum && r.Number != rowsSum.Length).Select(r => r.Sum).ToArray();

        public string NextSum => string.Format("{0:#,0}", rowsSum[numberQuestion - 1].Sum);

        public TableSums(Size size) : base(size)
        {
            using (Stream stream = ResourceProcessing.GetStream("Sums.json", TypeResource.Sums))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonText = reader.ReadToEnd();
                int[] sums = JsonConvert.DeserializeObject<int[]>(jsonText);

                BackgroundImageLayout = ImageLayout.Stretch;
                BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Background_Amounts.png"), size);

                tableSums = new TableLayoutPanel();
                rowsSum = new RowTableSum[sums.Length];

                int heightRow = (int)(size.Height * 0.67f / sums.Length);

                tableSums.Size = new Size((int)(0.8f * size.Width), heightRow * sums.Length + 1);
                tableSums.Location = new Point((Width - tableSums.Width) / 2, (int)(0.2f * size.Height));
                tableSums.BackColor = Color.Transparent;

                tableSums.RowCount = sums.Length;

                for (int i = 0; i < tableSums.RowCount; i++)
                    tableSums.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / tableSums.RowCount));

                tableSums.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                for (int i = sums.Length - 1; i > -1; i--)
                {
                    rowsSum[i] = new RowTableSum(i + 1, sums[i]);
                    tableSums.Controls.Add(rowsSum[i], 0, sums.Length - rowsSum[i].Number);
                }

                Controls.Add(tableSums);
            }
        }

        public void Reset(Mode mode)
        {
            X = MainForm.RectScreen.Width;

            tableSums.Visible = false;

            foreach (var row in rowsSum)
                row.Reset();

            if (mode == Mode.Classic)
                foreach (var row in rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            Prize = 0;
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
            RowTableSum saveSum = sender as RowTableSum;

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
            tableSums.Visible = true;

            await Task.Delay(1000);

            for (int i = 0; i < rowsSum.Length; i++)
            {
                try
                {
                    rowsSum[i].IsSelected = true;
                    rowsSum[i - 1].IsSelected = false;
                    rowsSum[i - 1].IconVisible = true;
                }
                catch (IndexOutOfRangeException) { }
                finally
                {
                    await Task.Delay(250);
                }
            }

            rowsSum[rowsSum.Length - 1].IconVisible = true;
        }

        public async Task ShowSaveSums()
        {
            foreach (var row in rowsSum)
            {
                row.IconVisible = false;
                row.IsSelected = false;
            }

            foreach (var row in rowsSum)
            {
                row.IconVisible = true;

                if (row.IsSaveSum)
                {
                    row.IsSelected = true;

                    await Task.Delay(1000);

                    row.IsSelected = false;
                }
            }

            rowsSum[rowsSum.Length - 1].IsSelected = true;
        }

        public void AnswerGiven(bool isCorrect)
        {
            if (isCorrect)
            {
                Prize = rowsSum[numberQuestion - 1].Sum;
                NumberQuestion++;
            }
            else
            {
                numberQuestion = rowsSum.Where(r => r.IsSaveSum && r.Number < numberQuestion).Select(r => r.Number).DefaultIfEmpty(0).Last();
                SetSelectedSum(numberQuestion);
                Prize = numberQuestion > 0 ? rowsSum[numberQuestion - 1].Sum : 0;
            }
        }
    }
}
