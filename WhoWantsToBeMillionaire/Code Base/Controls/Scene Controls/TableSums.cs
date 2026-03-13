using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableSums : TableLayoutPanel, IResettable
    {
        private readonly RowTableSums[] _rowsSum;

        private int _numberQuestion;
        private bool _taskCanceled;

        public TableSums()
        {
            var sums = JsonReader.GetObject<int[]>(Resources.Sums);
            int indexRow;

            _rowsSum = new RowTableSums[sums.Length];

            RowCount = sums.Length;

            for (int i = 0; i < RowCount; i++)
            {
                indexRow = RowCount - i - 1;
                _rowsSum[indexRow] = new RowTableSums(indexRow + 1, sums[indexRow]);

                RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                Controls.Add(_rowsSum[indexRow]);
            }
        }

        public event Action<int> SavingSumSelected;

        public int Prize { get; private set; }

        public string TextPrize { get; private set; }

        public string NextSum => string.Format("{0:#,0}", _rowsSum[Math.Min(_numberQuestion - 1, _rowsSum.Length - 1)].Sum);

        public int[] SavingSums => _rowsSum.Where(row => row.IsSavingSum && row.Number < Question.MaxNumber).Select(row => row.Sum).ToArray();

        public bool IsCurrentSavingSum => _rowsSum[_numberQuestion - 1].IsSavingSum;

        public void Reset(Modes mode = Modes.Classic)
        {
            _taskCanceled = true;

            foreach (var row in _rowsSum)
            {
                row.Reset();
                row.Click -= SelectSavingSum;
            }

            if (mode == Modes.Classic)
                foreach (var row in _rowsSum)
                    row.IsSavingSum = row.Number % 5 == 0;

            _numberQuestion = 1;
            SetPrize(0);
        }

        public bool IsSavingSum(int number)
        {
            return number > 0 && number <= Question.MaxNumber && _rowsSum[number - 1].IsSavingSum;
        }

        public void SetSelectedSum(int number)
        {
            foreach (var row in _rowsSum)
            {
                row.IsSelected = row.Number == number;
                row.IsIconVisible = row.Number <= number;
            }
        }

        public void AddSelectionSavingSum()
        {
            foreach (var row in _rowsSum)
            {
                row.Reset();
                row.AddMouseEvents();
                row.Click += SelectSavingSum;
            }
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
                    row.IsIconVisible = true;
                }

                row = rows.Dequeue();

                await Task.Delay(250);

                if (_taskCanceled)
                {
                    Clear();
                    return;
                }
            }

            _rowsSum[_rowsSum.Length - 1].IsIconVisible = true;
        }

        public async Task ShowSavingSums()
        {
            _taskCanceled = false;

            Clear();

            foreach (var row in _rowsSum)
            {
                row.IsIconVisible = true;

                if (row.IsSavingSum)
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

        public void CancelTask()
        {
            _taskCanceled = true;
        }

        public void Update(bool isCorrectAnswer)
        {
            if (isCorrectAnswer)
            {
                SetPrize(_numberQuestion);
                SetSelectedSum(_numberQuestion++);
            }
            else
            {
                int number = _rowsSum.Where(row => row.IsSavingSum && row.Number < _numberQuestion).Select(row => row.Number).DefaultIfEmpty(0).Last();
                SetSelectedSum(number);
                SetPrize(number);
            }
        }

        public void Clear()
        {
            foreach (var row in _rowsSum)
                row.IsSelected = row.IsIconVisible = false;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Height = Height / RowCount * RowCount + 1;
        }

        private void SetPrize(int number)
        {
            try
            {
                Prize = _rowsSum[number - 1].Sum;
                TextPrize = number < _rowsSum.Length ? string.Format("{0:#,0}", Prize) : "МИЛЛИОНЕР!";
            }
            catch (IndexOutOfRangeException)
            {
                Prize = 0;
                TextPrize = "0";
            }
        }

        private void SelectSavingSum(object sender, EventArgs e)
        {
            if (sender is RowTableSums SavingSum)
            {
                SetSelectedSum(SavingSum.Number);

                foreach (var row in _rowsSum)
                {
                    row.Click -= SelectSavingSum;
                    row.RemoveMouseEvents();
                }

                _rowsSum[_rowsSum.Length - 1].IsSavingSum = SavingSum.IsSavingSum = true;

                Sound.Play(Resources.SavaSumSelected);
                SavingSumSelected?.Invoke(SavingSum.Sum);
            }
        }
    }
}