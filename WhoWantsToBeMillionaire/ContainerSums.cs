using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ContainerSums : ControlAnimation
    {
        private int numberQuestion;

        private readonly TableLayoutPanel tableSums;
        private readonly RowSum[] rowsSum;

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

        public ContainerSums(Size size) : base(size)
        {
            using (Stream stream = ResourceProcessing.GetStream("Sums.json", TypeResource.Sums))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonText = reader.ReadToEnd();
                int[] sums = JsonConvert.DeserializeObject<int[]>(jsonText);

                BackgroundImageLayout = ImageLayout.Stretch;
                BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Background_Amounts.png"), size);

                tableSums = new TableLayoutPanel();
                rowsSum = new RowSum[sums.Length];

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
                    rowsSum[i] = new RowSum(i + 1, sums[i]);
                    tableSums.Controls.Add(rowsSum[i], 0, sums.Length - rowsSum[i].Number);
                }

                Controls.Add(tableSums);
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
            RowSum saveSum = sender as RowSum;

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

        public void Reset()
        {
            Location = new Point(MainForm.RectScreen.Width, 0);

            tableSums.Visible = false;

            foreach (var row in rowsSum)
                row.Reset();

            if ((Mode)Settings.Default.Mode == Mode.Classic)
                foreach (var row in rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            Prize = 0;
        }

        public async new Task Show()
        {
            int countFrames = 10;
            int dx = Width / countFrames;

            while (countFrames-- > 0)
            {
                Location = new Point(Location.X - dx, 0);
                await Task.Delay(MainForm.DeltaTime);
            }

            Location = new Point(MainForm.RectScreen.Width - Width, 0);
        }

        public async Task ShowControls()
        {
            tableSums.Visible = true;

            await Task.Delay(1000);

            int delay = 4 * MainForm.DeltaTime;

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
                    await Task.Delay(delay);
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
