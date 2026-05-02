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
        private readonly Dictionary<int, RowTableSums> _rowsSum;

        private int _numberQuestion;
        private bool _isTaskCanceled;

        public TableSums()
        {
            var sums = JsonReader.GetDictionary<int, int>(Resources.Dictionary_Sums);

            _rowsSum = sums.ToDictionary(pair => pair.Key, pair => new RowTableSums(pair.Key, pair.Value));

            RowCount = _rowsSum.Count();

            foreach (var row in _rowsSum.Values.OrderByDescending(row => row.NumberQuestion))
            {
                RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
                Controls.Add(row);
            }
        }

        public event Action<int> SavingSumSelected;

        public int Prize { get; private set; }

        public string TextPrize { get; private set; }

        public string NextSum => string.Format("{0:#,0}", _rowsSum[NumberQuestion].Sum);

        public int[] SavingSums => _rowsSum.Values.Where(row => row.IsSavingSum && row.NumberQuestion < Question.MaxNumber).Select(row => row.Sum).OrderBy(_ => _).ToArray();

        public bool IsCurrentSavingSum => _rowsSum[NumberQuestion].IsSavingSum;

        private int NumberQuestion
        {
            get => _numberQuestion;
            set => _numberQuestion = _rowsSum.ContainsKey(value) ? value : _rowsSum.First().Key;
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            _isTaskCanceled = true;

            foreach (var row in _rowsSum.Values)
            {
                row.Reset();
                row.Click -= SelectSavingSum;
            }

            var modesData = JsonReader.GetDictionary<Modes, ModeData>(Resources.Dictionary_ModeData);
            var numbersQuestionsSavingSums = modesData[mode].NumbersQuestionsSavingSums;

            foreach (var row in _rowsSum.Values)
                row.IsSavingSum = numbersQuestionsSavingSums.Contains(row.NumberQuestion);
            
            if(numbersQuestionsSavingSums.Length > 0)
                _rowsSum[Question.MaxNumber].IsSavingSum = true;

            NumberQuestion = Question.MinNumber;
            SetPrize(0);
        }

        public bool IsSavingSum(int questionNumber)
        {
            return _rowsSum.ContainsKey(questionNumber) && _rowsSum[questionNumber].IsSavingSum;
        }

        public void SetSelectedSum(int questionNumber)
        {
            foreach (var row in _rowsSum.Values)
            {
                row.IsSelected = row.NumberQuestion == questionNumber;
                row.IsIconVisible = row.NumberQuestion <= questionNumber;
            }
        }

        public void EnableSelectionSavingSum()
        {
            foreach (var row in _rowsSum.Values)
            {
                row.Reset();
                row.AddMouseEvents();
                row.Click += SelectSavingSum;
            }
        }

        public async Task ShowSums()
        {
            _isTaskCanceled = false;

            Queue<RowTableSums> rows = new Queue<RowTableSums>(_rowsSum.Values.OrderBy(row => row.NumberQuestion));
            RowTableSums currentRow = null;

            while (rows.Count > 0)
            {
                rows.Peek().IsSelected = true;

                if (currentRow != null)
                {
                    currentRow.IsSelected = false;
                    currentRow.IsIconVisible = true;
                }

                currentRow = rows.Dequeue();

                await Task.Delay(250);

                if (_isTaskCanceled)
                {
                    Clear();
                    return;
                }
            }

            _rowsSum[Question.MaxNumber].IsIconVisible = true;
        }

        public async Task ShowSavingSums()
        {
            _isTaskCanceled = false;

            Clear();

            var rows = _rowsSum.Values.Where(row => row.IsSavingSum).OrderBy(row => row.NumberQuestion);

            foreach (var row in rows)
            {
                SetSelectedSum(row.NumberQuestion);
                await Task.Delay(1000);

                if (_isTaskCanceled)
                {
                    Clear();
                    return;
                }
            }

            _rowsSum[Question.MaxNumber].IsSelected = true;
        }

        public void CancelTask()
        {
            _isTaskCanceled = true;
        }

        public void UpdateNumberQuestion(bool isCorrectAnswer)
        {
            if (isCorrectAnswer)
            {
                SetPrize(NumberQuestion);
                SetSelectedSum(NumberQuestion++);
            }
            else
            {
                int number = _rowsSum.Values.Where(row => row.IsSavingSum && row.NumberQuestion < NumberQuestion).Select(row => row.NumberQuestion).DefaultIfEmpty(0).Last();
                SetSelectedSum(number);
                SetPrize(number);
            }
        }

        public void Clear()
        {
            foreach (var row in _rowsSum.Values)
                row.IsSelected = row.IsIconVisible = false;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Height = Height / RowCount * RowCount + 1;
        }

        private void SetPrize(int questionNumber)
        {
            if (_rowsSum.ContainsKey(questionNumber))
            {
                Prize = _rowsSum[questionNumber].Sum;
                TextPrize = questionNumber < Question.MaxNumber ? string.Format("{0:#,0}", Prize) : "МИЛЛИОНЕР!";
            }
            else
            {
                Prize = 0;
                TextPrize = "0";
            }
        }

        private void SelectSavingSum(object sender, EventArgs e)
        {
            if (sender is RowTableSums selectedRow)
            {
                SetSelectedSum(selectedRow.NumberQuestion);

                foreach (var row in _rowsSum.Values)
                {
                    row.Click -= SelectSavingSum;
                    row.RemoveMouseEvents();
                }

                _rowsSum[Question.MaxNumber].IsSavingSum = selectedRow.IsSavingSum = true;

                Sound.Play(Resources.SavaSumSelected);
                SavingSumSelected?.Invoke(selectedRow.Sum);
            }
        }
    }
}