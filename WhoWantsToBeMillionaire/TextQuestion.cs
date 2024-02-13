using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum AnswerMode
    {
        Usual,
        DoubleDips,
        SwitchQuestion,
        TakeMoney
    }

    class TextQuestion : MovingPictureBox
    {
        private readonly Graphics g;
        private readonly Bitmap image;
        private readonly PictureText pictureTextQuestion;
        private readonly Bitmap wires;
        private readonly Dictionary<Letter, Option> options;
        private readonly CentralIconHint iconHint;

        public delegate void EventOptionClick(string explanation);
        public event EventOptionClick OptionClick;

        public Question Question { private set; get; }

        public AnswerMode AnswerMode { private set; get; }

        public bool IsCorrectAnswer { private set; get; }

        public TextQuestion(Size size) : base(size)
        {
            image = new Bitmap(size.Width, size.Height);
            wires = new Bitmap(size.Width, size.Height);
            g = Graphics.FromImage(image);

            int opWidth = (int)(0.45f * size.Width);

            Bitmap qImage = (Bitmap)ResourceProcessing.GetImage("Question.png");
            Bitmap opImage = (Bitmap)ResourceProcessing.GetImage("Option_Blue.png");

            Rectangle qRectangle = new Rectangle(0, 0, size.Width, qImage.Height * size.Width / qImage.Width);
            Size opSize = new Size(opWidth, opImage.Height * opWidth / opImage.Width);

            Font qFont = new Font("", 0.32f * opSize.Height);
            Font opFont = new Font("", 0.3f * opSize.Height);

            StringFormat qFormat = new StringFormat();
            StringFormat opFormat = new StringFormat();

            qFormat.Alignment = StringAlignment.Center;
            qFormat.LineAlignment = StringAlignment.Center;

            opFormat.Alignment = StringAlignment.Near;
            opFormat.LineAlignment = StringAlignment.Center;

            pictureTextQuestion = new PictureText(qRectangle, qFont, qFormat);
            options = new Dictionary<Letter, Option>();
            Letter[] keys = Enum.GetValues(typeof(Letter)).Cast<Letter>().ToArray();

            int dy = (int)(0.1f * opSize.Height);
            int x, y;
            Rectangle rect;

            for (int i = 0; i < keys.Length; i++)
            {
                x = Width / 2 - (i & 1 ^ 1) * opSize.Width;
                y = qRectangle.Height + i / 2 * (opSize.Height + dy) + dy;

                rect = new Rectangle(x, y, opSize.Width, opSize.Height);

                Option option = new Option(rect, keys[i], opFont, opFormat);
                options.Add(option.Letter, option);
            }

            Bitmap background = new Bitmap(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage(background))
            {
                g.DrawImage(qImage, qRectangle);

                foreach (var op in options.Values)
                    g.DrawImage(opImage, op.Rectangle);

                BackgroundImage = background;
            }

            using (Graphics g = Graphics.FromImage(wires))
            using (Bitmap wire = new Bitmap(ResourceProcessing.GetImage("Wire.png")))
            {
                int yWire = 0;

                foreach (var op in options.Values)
                    if (yWire != op.Rectangle.Y)
                    {
                        yWire = op.Rectangle.Y;
                        g.DrawImage(wire, 0, yWire, wires.Width, op.Rectangle.Height);
                    }

                foreach (var op in options.Values)
                    g.DrawImage(opImage, op.Rectangle);
            }

            MouseUp += OnMouseUp;

            qImage.Dispose();
            opImage.Dispose();

            int height = opSize.Height + dy;

            iconHint = new CentralIconHint();
            iconHint.Size = new Size((int)(1.6f * height), height);
            iconHint.Location = new Point((size.Width - iconHint.Width) / 2, qRectangle.Height + dy + opSize.Height / 2);
            iconHint.Visible = false;

            Controls.Add(iconHint);
        }

        public void Reset()
        {
            X = -Width * 3 / 2;
            Image = null;
            AnswerMode = AnswerMode.Usual;
        }

        public async Task ShowQuestion(int number, int countFrames)
        {
            await ShowQuestion(number, Question.RandomIndex(number), countFrames);
        }

        public async Task ShowQuestion(int number, int index, int countFrames)
        {
            Question = new Question(number, index);
            g.Clear(Color.Transparent);

            Bitmap[] frames = ResourceProcessing.FramesAppearance(wires, countFrames);

            foreach (var frame in frames)
            {
                g.DrawImage(wires, 0, 0);
                frame.Dispose();

                Image = image;
                await Task.Delay(MainForm.DeltaTime);
            }

            int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).ToArray();

            pictureTextQuestion.Reset();
            pictureTextQuestion.Text = Question.Text;

            foreach (var a in alphas)
            {
                pictureTextQuestion.Alpha = a;
                g.DrawImage(pictureTextQuestion.ImageText, pictureTextQuestion.Rectangle);

                Image = image;
                await Task.Delay(MainForm.DeltaTime);
            }

            await Task.Delay(1000);

            foreach (var op in options.Values)
            {
                op.Reset();
                op.Text = Question.Options[op.Letter];

                foreach (var a in alphas)
                {
                    op.Alpha = a;
                    g.DrawImage(op.ImageText, op.Rectangle);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

                await Task.Delay(1000);
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            foreach (var op in options.Values)
                if (op.Enabled && op.Rectangle.Contains(e.X, e.Y))
                {
                    OnOptionClick(op.Letter);
                    return;
                }
        }

        private async void OnOptionClick(Letter letter)
        {
            Option option = options[letter];
            SelectOption(letter);

            IsCorrectAnswer = Question.Correct == letter;

            if (AnswerMode == AnswerMode.DoubleDips && !IsCorrectAnswer)
            {
                AnswerMode = AnswerMode.Usual;
                await Task.Delay(3000);

                option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
                option.Enabled = false;

                using (Bitmap lockedOption = (Bitmap)ResourceProcessing.GetImage("Option_Gray.png"))
                    g.DrawImage(lockedOption, option.Rectangle);

                g.DrawImage(option.ImageText, option.Rectangle);

                Image = image;

                if (Question.CountOptions == 2)
                {
                    await Task.Delay(3000);
                    IsCorrectAnswer = true;
                    SelectOption(Question.Correct);
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

        private async void SelectOption(Letter letter)
        {
            Option option = options[letter];

            option.Selected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (Bitmap selectedOption = (Bitmap)ResourceProcessing.GetImage("Option_Orange.png"))
            {
                Bitmap[] frames = ResourceProcessing.FramesAppearance(selectedOption, 6);

                foreach (var img in frames)
                {
                    g.DrawImage(img, option.Rectangle);
                    g.DrawImage(option.ImageText, option.Rectangle);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

                foreach (var img in frames)
                    img.Dispose();
            }
        }

        public async Task Clear(int countFrames)
        {
            if (iconHint.Visible)
            {
                g.DrawImage(iconHint.BackgroundImage, new Rectangle(iconHint.Location, iconHint.Size));
                Image = image;
                iconHint.Visible = false;
                iconHint.Clear();
            }

            Bitmap[] frames = ResourceProcessing.FramesDisappearance(image, countFrames);

            foreach (var img in frames)
            {
                Image = img;
                await Task.Delay(MainForm.DeltaTime);
            }

            g.Clear(Color.Transparent);
            Image = null;

            foreach (var img in frames)
                img.Dispose();

            iconHint.Visible = false;
        }

        public async Task ShowCorrect(int countFrames)
        {
            Option option = options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (Bitmap startFrame = (Bitmap)ResourceProcessing.GetImage(option.Selected ? "Option_Orange.png" : "Option_Blue.png"))
            using (Bitmap finalFrame = (Bitmap)ResourceProcessing.GetImage("Option_Green.png"))
            {
                Bitmap[] frames = ResourceProcessing.FramesTransition(startFrame, finalFrame, countFrames);

                int n = frames.Length - 1;
                int n2 = 2 * n;
                int n5 = 5 * n;

                for (int i = 0; i <= n5; i++)
                {
                    g.DrawImage(frames[n - Math.Abs(i % n2 - n)], option.Rectangle);
                    g.DrawImage(option.ImageText, option.Rectangle);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

                foreach (var img in frames)
                    img.Dispose();
            }
        }

        public async Task ShowCentralIcon(TypeHint hint)
        {
            iconHint.Visible = true;
            await iconHint.ShowIcon(hint);
        }

        public async Task HideCentralIcon()
        {
            await iconHint.HideIcon();
            iconHint.Visible = iconHint.BackgroundImage != null;
        }

        public async void OnHintClick(TypeHint hint)
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

                    g.Clear(Color.Transparent);
                    g.DrawImage(pictureTextQuestion.ImageText, pictureTextQuestion.Rectangle);

                    foreach (var op in options.Values)
                    {
                        op.Text = Question.Options[op.Letter];
                        op.Enabled = op.Text != string.Empty;
                        g.DrawImage(op.ImageText, op.Rectangle);
                    }

                    Image = image;
                    break;

                case TypeHint.PhoneFriend:
                case TypeHint.AskAudience:
                    Enabled = false;
                    break;

                case TypeHint.DoubleDip:
                    AnswerMode = AnswerMode.DoubleDips;
                    await ShowCentralIcon(hint);
                    break;

                case TypeHint.SwitchQuestion:
                    AnswerMode = AnswerMode.SwitchQuestion;
                    await ShowCentralIcon(hint);
                    break;

                case TypeHint.AskHost:
                    Enabled = false;
                    await ShowCentralIcon(hint);
                    break;
            }
        }
    }
}
