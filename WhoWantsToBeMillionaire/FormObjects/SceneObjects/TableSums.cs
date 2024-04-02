using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableSums : MovingPictureBox, IReset
    {
        private readonly TableLayoutPanel _table;
        private readonly RowTableSums[] _rowsSum;

        private bool _taskCanceled;
        private int _numberQuestion;

        public delegate void EventSaveSumSelected(int sum);
        public event EventSaveSumSelected SaveSumSelected;

        public int Prize { private set; get; }

        public string TextPrize { private set; get; }

        public string NextSum =>
            string.Format("{0:#,0}", _rowsSum[Math.Min(_numberQuestion - 1, _rowsSum.Length - 1)].Sum);

        public int[] SaveSums =>
            _rowsSum.Where(r => r.IsSaveSum && r.Number < Question.MaxNumber).Select(r => r.Sum).ToArray();

        public bool NowSaveSum =>
            _rowsSum[_numberQuestion - 1].IsSaveSum;

        public TableSums(int width, int height) : base(width, height)
        {
            BackgroundImage = new Bitmap(Resources.Background_Sums, width, height);

            var sums = JsonManager.GetObject<int[]>(Resources.Sums);
            var heightRow = (int)(height * 0.67f / sums.Length);

            _table = new TableLayoutPanel();
            _rowsSum = new RowTableSums[sums.Length];

            _table.Size = new Size((int)(0.8f * width), heightRow * sums.Length + 1);
            _table.Location = new Point((Width - _table.Width) / 2, (int)(0.2f * height));
            _table.RowCount = sums.Length;

            for (int i = 0; i < _table.RowCount; i++)
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / _table.RowCount));

            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            for (int i = sums.Length - 1; i > -1; i--)
            {
                _rowsSum[i] = new RowTableSums(i + 1, sums[i]);
                _table.Controls.Add(_rowsSum[i], 0, sums.Length - _rowsSum[i].Number);
            }

            Controls.Add(_table);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            X = MainForm.ScreenRectangle.Width;

            _taskCanceled = true;
            _table.Visible = false;

            foreach (var row in _rowsSum)
                row.Reset();

            if (mode == Mode.Classic)
                foreach (var row in _rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            _numberQuestion = 1;
            SetPrize(0);
        }

        public async new Task Show()
        {
            await MoveX(MainForm.ScreenRectangle.Width - Width, 600 / MainForm.DeltaTime);
            _table.Visible = true;
        }

        private void SetPrize(int numberSum)
        {
            try
            {
                Prize = _rowsSum[numberSum - 1].Sum;
                TextPrize = numberSum < _rowsSum.Length ? string.Format("{0:#,0}", Prize) : "МИЛЛИОНЕР!";
            }
            catch (IndexOutOfRangeException)
            {
                Prize = 0;
                TextPrize = "0";
            }
        }

        public void SetSelectedSum(int number)
        {
            foreach (var row in _rowsSum)
            {
                row.IsSelected = row.Number == number;
                row.IconVisible = row.Number <= number;
            }
        }

        public void AddSelectionSaveSum()
        {
            foreach (var row in _rowsSum)
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

            foreach (var row in _rowsSum)
            {
                row.Click -= SelectSaveSum;
                row.RemoveMouseEvents();
            }

            _rowsSum[_rowsSum.Length - 1].IsSaveSum = saveSum.IsSaveSum = true;

            Sound.Play(Resources.SavaSumSelected);
            SaveSumSelected.Invoke(saveSum.Sum);
        }

        public async Task ShowSums()
        {
            _taskCanceled = false;

            Queue<RowTableSums> rows = new Queue<RowTableSums>(_rowsSum);
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

                if (_taskCanceled)
                {
                    Clear();
                    return;
                }
            }

            _rowsSum[_rowsSum.Length - 1].IconVisible = true;
        }

        public async Task ShowSaveSums()
        {
            _taskCanceled = false;

            Clear();

            foreach (var row in _rowsSum)
            {
                row.IconVisible = true;

                if (row.IsSaveSum)
                {
                    row.IsSelected = true;

                    await Task.Delay(1000);

                    row.IsSelected = false;
                }

                if (_taskCanceled)
                {
                    Clear();
                    return;
                }
            }

            _rowsSum[_rowsSum.Length - 1].IsSelected = true;
        }

        public void CancelTask() =>
            _taskCanceled = true;

        public void Update(bool isCorrectAnswer)
        {
            if (isCorrectAnswer)
            {
                SetPrize(_numberQuestion);
                SetSelectedSum(_numberQuestion++);
            }
            else
            {
                int number = _rowsSum.Where(r => r.IsSaveSum && r.Number < _numberQuestion).Select(r => r.Number).DefaultIfEmpty(0).Last();
                SetSelectedSum(number);
                SetPrize(number);
            }
        }

        public void Clear()
        {
            foreach (var row in _rowsSum)
                row.IsSelected = row.IconVisible = false;
        }
    }
}