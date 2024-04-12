using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class TableSums : TableLayoutPanel, IReset
    {
        private readonly RowTableSums[] _rowsSum;

        private int _numberQuestion;
        private bool _taskCanceled;

        public Action<int> SaveSumSelected;

        public int Prize { get; private set; }

        public string TextPrize { get; private set; }

        public string NextSum =>
            string.Format("{0:#,0}", _rowsSum[Math.Min(_numberQuestion - 1, _rowsSum.Length - 1)].Sum);

        public int[] SaveSums =>
            _rowsSum.Where(r => r.IsSaveSum && r.Number < Question.MaxNumber).Select(r => r.Sum).ToArray();

        public bool NowSaveSum =>
            _rowsSum[_numberQuestion - 1].IsSaveSum;

        public TableSums()
        {
            var sums = JsonManager.GetObject<int[]>(Resources.Settings_Sums);

            _rowsSum = new RowTableSums[sums.Length];
            RowCount = sums.Length;

            int indexRow;

            for (int i = 0; i < RowCount; i++)
            {
                indexRow = RowCount - i - 1;
                _rowsSum[indexRow] = new RowTableSums(indexRow + 1, sums[indexRow]);

                RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                Controls.Add(_rowsSum[indexRow]);
            }
        }

        protected override void OnSizeChanged(EventArgs e) =>
            Height = Height / RowCount * RowCount + 1;

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

        private void SelectSaveSum(object sender, EventArgs e)
        {
            var saveSum = sender as RowTableSums;

            SetSelectedSum(saveSum.Number);

            foreach (var row in _rowsSum)
            {
                row.Click -= SelectSaveSum;
                row.RemoveMouseEvents();
            }

            _rowsSum[_rowsSum.Length - 1].IsSaveSum = saveSum.IsSaveSum = true;

            Sound.Play(Resources.SavaSumSelected);
            SaveSumSelected?.Invoke(saveSum.Sum);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            _taskCanceled = true;

            foreach (var row in _rowsSum)
            { 
                row.Reset();
                row.Click -= SelectSaveSum;
            }

            if (mode == Mode.Classic)
                foreach (var row in _rowsSum)
                    row.IsSaveSum = row.Number % 5 == 0;

            _numberQuestion = 1;
            SetPrize(0);
        }

        public bool CheckSaveSum(int number) => 
            number > 0 && number <= Question.MaxNumber && _rowsSum[number - 1].IsSaveSum;

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