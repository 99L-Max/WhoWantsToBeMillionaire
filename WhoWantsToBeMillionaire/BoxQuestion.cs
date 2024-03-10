using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

    class BoxQuestion : GameContol, IReset, IGameSettings
    {
        private const int CountFramesAlphaChange = 6;

        private readonly Graphics g;
        private readonly Bitmap image;
        private readonly Bitmap wires;
        private readonly BitmapText questionText;
        private readonly Dictionary<Letter, Option> options;
        private readonly CentralIconHint iconHint;

        private bool isShowOptionsSequentially = true;

        public delegate void EventOptionClick(Letter letter);
        public event EventOptionClick OptionClick;

        public AnswerMode AnswerMode;

        public Question Question { private set; get; }

        public bool IsCorrectAnswer { private set; get; }

        public BoxQuestion(int width, int height) : base(width, height)
        {
            image = new Bitmap(width, height);
            wires = new Bitmap(width, height);
            options = new Dictionary<Letter, Option>();
            iconHint = new CentralIconHint();
            g = Graphics.FromImage(image);

            int opWidth = (int)(0.45f * width);

            Bitmap qImage = (Bitmap)ResourceProcessing.GetImage("Question.png");
            Bitmap opImage = (Bitmap)ResourceProcessing.GetImage("ButtonWire_Blue.png");

            Rectangle qRectangle = new Rectangle(0, 0, width, qImage.Height * width / qImage.Width);
            Size opSize = new Size(opWidth, opImage.Height * opWidth / opImage.Width);

            questionText = new BitmapText(qRectangle);
            questionText.SizeFont = 0.32f * opSize.Height;

            Letter[] keys = Enum.GetValues(typeof(Letter)).Cast<Letter>().ToArray();

            int dy = (int)(0.1f * opSize.Height);
            int x, y;

            Rectangle rect;
            Option option;

            for (int i = 0; i < keys.Length; i++)
            {
                x = Width / 2 - (i & 1 ^ 1) * opSize.Width;
                y = qRectangle.Height + i / 2 * (opSize.Height + dy) + dy;

                rect = new Rectangle(x, y, opSize.Width, opSize.Height);

                option = new Option(rect, keys[i]);
                option.SizeFont = 0.3f * opSize.Height;
                options.Add(option.Letter, option);
            }

            Bitmap background = new Bitmap(width, height);

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
                y = 0;

                foreach (var op in options.Values)
                    if (y != op.Rectangle.Y)
                    {
                        y = op.Rectangle.Y;
                        g.DrawImage(wire, 0, y, wires.Width, op.Rectangle.Height);
                    }

                foreach (var op in options.Values)
                    g.DrawImage(opImage, op.Rectangle);
            }

            qImage.Dispose();
            opImage.Dispose();

            int iconHeight = opSize.Height + dy;

            iconHint.Size = new Size((int)(1.6f * iconHeight), iconHeight);
            iconHint.Location = new Point((width - iconHint.Width) / 2, qRectangle.Height + dy + opSize.Height / 2);
            iconHint.Visible = false;

            Controls.Add(iconHint);

            MouseUp += OnMouseUp;
        }

        public void Reset(Mode? mode = null)
        {
            questionText.Reset();

            foreach (var op in options.Values)
                op.Reset();

            Enabled = false;
            Image = null;
            AnswerMode = AnswerMode.Usual;
        }

        private void DrawFrame(Image frame, Rectangle rectangle, int numberFrame, int countFrames)
        {
            using (ImageAttributes attribute = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix { Matrix33 = (numberFrame + 1f) / countFrames };
                attribute.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(frame, rectangle, 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel, attribute);
            }
        }

        public void SetQuestion(int number) => SetQuestion(new Question(number));

        public void SetQuestion(int number, int index) => SetQuestion(new Question(number, index));

        public void SetQuestion(Question question)
        {
            Question = question;

            g.Clear(Color.Transparent);
            g.DrawImage(wires, 0, 0, Width, Height);

            questionText.Text = question.Text;
            g.DrawImage(questionText.ImageText, questionText.Rectangle);

            foreach (var op in options.Values)
            {
                op.Text = question.Options[op.Letter];
                op.Enabled = op.Text != string.Empty;
                g.DrawImage(op.ImageText, op.Rectangle);
            }

            Image = image;
        }

        public async Task ShowQuestion()
        {
            Rectangle rectWires = new Rectangle(0, 0, Width, Height);

            for (int i = 1; i <= CountFramesAlphaChange; i++)
            {
                DrawFrame(wires, rectWires, i, CountFramesAlphaChange);

                Image = image;
                await Task.Delay(MainForm.DeltaTime);
            }

            int delay = 250 + 500 * (int)Question.Difficulty;
            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            foreach (var a in alphas)
            {
                questionText.Alpha = a;
                g.DrawImage(questionText.ImageText, questionText.Rectangle);

                Image = image;
                await Task.Delay(MainForm.DeltaTime);
            }

            await Task.Delay(delay);

            foreach (var op in options.Values)
            {
                foreach (var a in alphas)
                {
                    op.Alpha = a;
                    g.DrawImage(op.ImageText, op.Rectangle);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

                await Task.Delay(delay);
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

        private void OnOptionClick(Letter letter)
        {
            Enabled = false;
            SelectOption(letter);

            if (Question.Difficulty != DifficultyQuestion.Easy && (AnswerMode == AnswerMode.Usual || AnswerMode == AnswerMode.DoubleDips))
            {
                Sound.Play("Answer_Accepted.wav");
                Sound.PlayBackground("Answer_DrumRoll.wav");
            }

            OptionClick.Invoke(letter);
        }

        private async void SelectOption(Letter letter)
        {
            IsCorrectAnswer = letter == Question.Correct;

            Option option = options[letter];

            option.Selected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (Image selectedOption = ResourceProcessing.GetImage("ButtonWire_Orange.png"))
            {
                for (int i = 1; i <= CountFramesAlphaChange; i++)
                {
                    DrawFrame(selectedOption, option.Rectangle, i, CountFramesAlphaChange);

                    g.DrawImage(option.ImageText, option.Rectangle);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }
            }
        }

        public void LockOption(Letter letter)
        {
            Option option = options[letter];

            option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
            option.Selected = option.Enabled = false;

            using (Image lockedOption = ResourceProcessing.GetImage("ButtonWire_Gray.png"))
                g.DrawImage(lockedOption, option.Rectangle);

            g.DrawImage(option.ImageText, option.Rectangle);

            Image = image;

            Sound.Play($"Answer_Incorrect_{Question.Difficulty}.wav");
        }

        public void ClickCorrect()
        {
            OnOptionClick(Question.Correct);
        }

        public async Task Clear()
        {
            if (iconHint.Visible)
            {
                g.DrawImage(iconHint.BackgroundImage, new Rectangle(iconHint.Location, iconHint.Size));

                Image = image;

                iconHint.Visible = false;
                iconHint.Clear();
            }

            Rectangle rectImage = new Rectangle(0, 0, Width, Height);

            using (Image mainImage = new Bitmap(image))
                for (int i = CountFramesAlphaChange - 1; i > 0; i--)
                {
                    g.Clear(Color.Transparent);
                    DrawFrame(mainImage, rectImage, i, CountFramesAlphaChange);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

            g.Clear(Color.Transparent);
            Image = image;

            Reset();
        }

        public async Task ShowCorrect(bool playSound, bool addDelay)
        {
            if (playSound)
                Sound.Play(IsCorrectAnswer ? $"Answer_Correct_{Question.Difficulty}.wav" : $"Answer_Incorrect_{Question.Difficulty}.wav");

            Option option = options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (Image startFrame = ResourceProcessing.GetImage(option.Selected ? "ButtonWire_Orange.png" : "ButtonWire_Blue.png"))
            using (Image finalFrame = ResourceProcessing.GetImage("ButtonWire_Green.png"))
            {
                Image frame, back;

                for (int stage = 0; stage < 5; stage++)
                {
                    (frame, back) = (stage & 1) == 0 ? (finalFrame, startFrame) : (startFrame, finalFrame);

                    for (int i = 1; i <= CountFramesAlphaChange; i++)
                    {
                        g.DrawImage(back, option.Rectangle);

                        DrawFrame(frame, option.Rectangle, i, CountFramesAlphaChange);

                        g.DrawImage(option.ImageText, option.Rectangle);

                        Image = image;
                        await Task.Delay(MainForm.DeltaTime);
                    }
                }
            }

            if (addDelay)
                await Task.Delay(3000);
        }

        public async Task ShowCentralIcon(TypeHint hint, bool playSound)
        {
            iconHint.Visible = true;
            await iconHint.ShowIcon(hint, playSound);
        }

        public async Task HideCentralIcon(bool playSound)
        {
            await iconHint.HideIcon(playSound);
            iconHint.Visible = iconHint.BackgroundImage != null;
        }

        public void PlayBackgroundSound(string soundName)
        {
            if (Question.Difficulty != DifficultyQuestion.Easy)
                Sound.PlayBackground(soundName);
        }

        public void SetSettings(GameSettingsData data)
        {
            isShowOptionsSequentially = (bool)data.GetSettings(GameSettings.ShowOptionsSequentially);
        }
    }
}
