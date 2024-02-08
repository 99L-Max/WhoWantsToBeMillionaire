using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WhoWantsToBeMillionaire
{
    enum AnswerMode
    {
        Usual,
        DoubleDips,
        SwitchQuestion,
        TakeMoney
    }

    class ContainerQuestion : ControlAnimation
    {
        private readonly TextPictureBox textPrize;
        private readonly TextPictureBox textQuestion;
        private readonly Dictionary<Letter, Option> options;
        private readonly CentralIconHint iconHint;

        public delegate void EventOptionClick(string explanation);
        public event EventOptionClick OptionClick;

        public Question Question { private set; get; }

        public AnswerMode AnswerMode { private set; get; }

        public bool IsCorrectAnswer { private set; get; }

        public ContainerQuestion(Size size) : base(size)
        {
            Bitmap qImage = (Bitmap)ResourceProcessing.GetImage("Question.png");
            Bitmap opImage = CustomButton.ImageButton[ThemeButton.Blue];

            int opWidth = (int)(0.45f * size.Width);

            Size qSize = new Size(size.Width, qImage.Height * size.Width / qImage.Width);
            Size opSize = new Size(opWidth, opImage.Height * opWidth / opImage.Width);

            StringFormat qFormat = new StringFormat();
            StringFormat opFormat = new StringFormat();

            qFormat.Alignment = StringAlignment.Center;
            qFormat.LineAlignment = StringAlignment.Center;

            opFormat.Alignment = StringAlignment.Near;
            opFormat.LineAlignment = StringAlignment.Center;

            textQuestion = new TextPictureBox(qSize, qImage, 0.16f * qSize.Height, qFormat);
            textPrize = new TextPictureBox(qSize, qImage, 0.42f * qSize.Height, qFormat);

            textQuestion.Size = textPrize.Size = qSize;

            textPrize.Y = textPrize.Height / 2;

            int dy = (int)(0.1f * opImage.Height);

            options = new Dictionary<Letter, Option>();
            Letter[] keys = Enum.GetValues(typeof(Letter)).Cast<Letter>().ToArray();
            Option option;

            for (int i = 0; i < keys.Length; i++)
            {
                option = new Option(keys[i], opSize, opImage, 0.3f * opSize.Height, opFormat);
                option.Y = textQuestion.Height + i / 2 * (opSize.Height + dy) + dy;
                option.Click += OnOptionClick;

                options.Add(option.Letter, option);
                Controls.Add(option);
            }

            int height = opSize.Height + dy;

            iconHint = new CentralIconHint();
            iconHint.Size = new Size((int)(1.6f * height), height);
            iconHint.Location = new Point((size.Width - iconHint.Width) / 2, textQuestion.Height + opSize.Height / 2 + dy);
            iconHint.Visible = false;

            Controls.Add(textQuestion);
            Controls.Add(textPrize);
            Controls.Add(iconHint);

            Enabled = false;

            Reset();
        }

        public void Reset()
        {
            textPrize.X = -textQuestion.Width * 3 / 2;
            SetXQuestion(textPrize.X);
        }

        private void SetXQuestion(int x = 0)
        {
            int i = 0;

            textQuestion.X = x;
            foreach (var op in options.Values)
                op.X = textQuestion.Width / 2 - (i++ % 2 ^ 1) * op.Width + x;
        }

        private void SetTextQuestion()
        {
            textQuestion.Text = Question.Text;
            foreach (var op in options.Values)
            {
                op.Text = Question.Options[op.Letter];
                op.Enabled = op.Text != string.Empty;
            }
        }

        public async Task ShowQuestion(int number)
        {
            await ShowQuestion(number, Question.RandomIndex(number));
        }

        public async Task ShowQuestion(int number, int index)
        {
            Question = new Question(number, index);
            SetTextQuestion();

            int countFrames = 15;
            int dx = -textQuestion.X / countFrames;

            do
            {
                textQuestion.X += dx;
                foreach (var op in options.Values)
                    op.X += dx;

                await Task.Delay(MainForm.DeltaTime);
            }
            while (--countFrames > 0);

            SetXQuestion();

            countFrames = 6;

            await textQuestion.ShowText(countFrames);
            foreach (var op in options.Values)
            {
                await Task.Delay(1000);
                await op.ShowText(countFrames);
            }

            AnswerMode = AnswerMode.Usual;
            Enabled = true;
        }

        private async void OnOptionClick(object sender, EventArgs e)
        {
            Enabled = false;

            Option option = sender as Option;
            option.Choose();

            IsCorrectAnswer = Question.Correct == option.Letter;

            if (AnswerMode == AnswerMode.DoubleDips && !IsCorrectAnswer)
            {
                AnswerMode = AnswerMode.Usual;
                await Task.Delay(3000);
                option.Lock();

                if (Question.CountOptions == 2)
                {
                    await Task.Delay(3000);
                    IsCorrectAnswer = true;
                    options[Question.Correct].Choose();
                }
                else
                {
                    Enabled = true;
                    return;
                }
            }

            string explanation = Question.Explanation;
            if (!IsCorrectAnswer)
                explanation += $"\nПравильный ответ: {Question.FullCorrect}.";

            OptionClick.Invoke(explanation);
        }

        public async Task ShowCorrect()
        {
            await options[Question.Correct].Blink();
        }

        public async Task ShowPrize(string prize)
        {
            await ClearQuestion();

            int countFrames = 15;
            int dx = -textPrize.X / countFrames;

            do
            {
                textPrize.X += dx;
                textQuestion.X += dx;
                foreach (var op in options.Values)
                    op.X += dx;

                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            textPrize.X = 0;

            countFrames = 6;
            textPrize.Text = prize;
            await textPrize.ShowText(countFrames);
        }

        public async Task HidePrize()
        {
            await textPrize.HideText(6);

            int countFrames = 15;
            int dx = textPrize.Width / countFrames;

            do
            {
                textPrize.X += dx;
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            Reset();
        }

        public void HideCentralIcon()
        {
            iconHint.HideIcon();
        }

        private void ShowCentralIcon(TypeHint hint)
        {
            iconHint.Visible = true;
            iconHint.BringToFront();
            iconHint.ShowIcon(hint);
        }

        private async Task ClearQuestion()
        {
            int countFrames = 6;

            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() => textQuestion.HideText(countFrames)));
            foreach (var op in options.Values)
                tasks.Add(Task.Run(() => op.Clear(countFrames)));
            tasks.Add(Task.Run(() => iconHint.Clear(countFrames)));

            await Task.WhenAll(tasks);

            iconHint.Visible = false;
        }

        public async Task SwitchQuestionQuestion()
        {
            await options[Question.Correct].Blink();
            await Task.Delay(3000);
            await ClearQuestion();

            int countFrames = 15;
            int dx = (Width - textQuestion.X) / countFrames;

            do
            {
                textQuestion.X += dx;
                foreach (var op in options.Values)
                    op.X += dx;

                await Task.Delay(MainForm.DeltaTime);
            }
            while (--countFrames > 0);

            Reset();

            int newIndex;
            do
                newIndex = Question.RandomIndex(Question.Number);
            while (newIndex == Question.Index);

            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() => ShowCentralIcon(TypeHint.SwitchQuestion)));
            tasks.Add(Task.Run(() => ShowQuestion(Question.Number, newIndex)));

            await Task.WhenAll(tasks);
        }

        public void OnHintClick(TypeHint hint)
        {
            switch (hint)
            {
                case TypeHint.FiftyFifty:
                    Dictionary<Letter, string> dict = new Dictionary<Letter, string>();

                    var wrongKeys = Question.Options.Keys.Where(k => k != Question.Correct).ToList();
                    Letter secondKey = wrongKeys[new Random().Next(wrongKeys.Count)];

                    dict.Add(Question.Correct, Question.Options[Question.Correct]);
                    dict.Add(secondKey, Question.Options[secondKey]);

                    Question = new Question(Question.Number, Question.Index, Question.Text, dict, Question.Correct, Question.Explanation);
                    SetTextQuestion();
                    break;

                case TypeHint.PhoneFriend:
                case TypeHint.AskAudience:
                    Enabled = false;
                    break;

                case TypeHint.DoubleDip:
                    AnswerMode = AnswerMode.DoubleDips;
                    ShowCentralIcon(hint);
                    break;

                case TypeHint.SwitchQuestion:
                    AnswerMode = AnswerMode.SwitchQuestion;
                    ShowCentralIcon(hint);
                    break;

                case TypeHint.AskHost:
                    Enabled = false;
                    ShowCentralIcon(hint);
                    break;
            }
        }
    }
}
