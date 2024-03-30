using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

    class BoxQuestion : GameContol, IReset, IGameSettings
    {
        private const int CountFramesAlphaChange = 6;

        private readonly Graphics g;
        private readonly Image image;
        private readonly Image wires;
        private readonly TextBitmap textQuestion;
        private readonly Dictionary<Letter, Option> options;
        private readonly Dictionary<Letter, ButtonOption> buttons;
        private readonly CentralIconHint iconHint;
        private readonly List<TextBitmap> bitmapTexts;

        private bool isSequentially = true;

        public delegate void EventOptionClick(Letter letter);
        public event EventOptionClick OptionClick;

        public AnswerMode AnswerMode;

        public Question Question { private set; get; }

        public bool IsCorrectAnswer { private set; get; }

        public BoxQuestion(int width, int height) : base(width, height)
        {
            Rectangle rectQuestion = new Rectangle(0, 0, width, (int)(0.11f * width));
            Size sizeOption = new Size((int)(0.45f * width), rectQuestion.Height >> 1);

            image = new Bitmap(width, height);
            wires = new Bitmap(width, height);
            options = new Dictionary<Letter, Option>();
            iconHint = new CentralIconHint();
            bitmapTexts = new List<TextBitmap>();
            textQuestion = new TextBitmap(rectQuestion, 64);
            g = Graphics.FromImage(image);

            textQuestion.SizeFont = 0.45f * sizeOption.Height;

            Letter[] keys = Question.Letters.ToArray();
            Rectangle rectOption = new Rectangle(new Point(), sizeOption);
            Option option;
            int dy = (int)(0.1f * sizeOption.Height);

            for (int i = 0; i < keys.Length; i++)
            {
                rectOption.X = (Width >> 1) - (i & 1 ^ 1) * sizeOption.Width;
                rectOption.Y = rectQuestion.Height + (i >> 1) * (sizeOption.Height + dy) + dy;

                option = new Option(keys[i], rectOption);
                option.SizeFont = 0.4f * sizeOption.Height;
                options.Add(option.Letter, option);
            }

            bitmapTexts.Add(textQuestion);
            bitmapTexts = bitmapTexts.Concat(options.Values).ToList();

            buttons = options.ToDictionary(k => k.Key, v => new ButtonOption(v.Key, v.Value.Rectangle));

            foreach (var b in buttons.Values)
            {
                Controls.Add(b);
                b.Click += OnOptionClick;
            }

            Image background = new Bitmap(width, height);

            using (Graphics gBack = Graphics.FromImage(background))
            using (Graphics gWire = Graphics.FromImage(wires))
            using (Image wire = ResourceManager.GetImage("Wire.png"))
            using (Image questionBack = ResourceManager.GetImage("Question.png"))
            using (Image optionBack = ResourceManager.GetImage("ButtonWire_Blue.png"))
            {
                gBack.DrawImage(questionBack, rectQuestion);

                foreach (var op in options.Values)
                    gBack.DrawImage(optionBack, op.Rectangle);

                BackgroundImage = background;

                foreach (var yWire in options.Values.Select(t => t.Rectangle.Y).Distinct())
                    gWire.DrawImage(wire, 0, yWire, wires.Width, sizeOption.Height);

                foreach (var op in options.Values)
                    gWire.DrawImage(optionBack, op.Rectangle);
            }

            int iconHeight = sizeOption.Height + dy;

            iconHint.Size = new Size((int)(1.6f * iconHeight), iconHeight);
            iconHint.Location = new Point((width - iconHint.Width) >> 1, rectQuestion.Height + dy + (sizeOption.Height >> 1));
            iconHint.Visible = false;

            Controls.Add(iconHint);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            bitmapTexts.ForEach(t => t.Reset());
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

            textQuestion.Text = question.Text;
            g.DrawImage(textQuestion.ImageText, textQuestion.Rectangle);

            foreach (var op in options.Values)
            {
                op.Text = question.Options[op.Letter];
                op.Enabled = op.Text != string.Empty;

                buttons[op.Letter].Visible = op.Enabled;
                g.DrawImage(op.ImageText, op.Rectangle);
            }

            Image = image;
        }

        public async Task ShowQuestion()
        {
            Rectangle rectWires = new Rectangle(0, 0, Width, Height);

            for (int i = 1; i <= CountFramesAlphaChange; i++)
            {
                g.Clear(Color.Transparent);
                DrawFrame(wires, rectWires, i, CountFramesAlphaChange);

                Image = image;
                await Task.Delay(MainForm.DeltaTime);
            }

            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            if (isSequentially)
            {
                int delay = 250 + 500 * (int)Question.Difficulty;

                foreach (var text in bitmapTexts)
                {
                    foreach (var a in alphas)
                    {
                        text.Alpha = a;
                        g.DrawImage(text.ImageText, text.Rectangle);
                        Image = image;
                        await Task.Delay(MainForm.DeltaTime);
                    }

                    await Task.Delay(delay);
                }
            }
            else
            {
                foreach (var a in alphas)
                {
                    foreach (var text in bitmapTexts)
                    {
                        text.Alpha = a;
                        g.DrawImage(text.ImageText, text.Rectangle);
                    }

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }
            }
        }

        private void OnOptionClick(object sender, EventArgs e)
        {
            Enabled = false;

            Letter letter = (sender as ButtonOption).Letter;
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

            using (Image selectedOption = ResourceManager.GetImage("ButtonWire_Orange.png"))
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

            using (Image lockedOption = ResourceManager.GetImage("ButtonWire_Gray.png"))
                g.DrawImage(lockedOption, option.Rectangle);

            g.DrawImage(option.ImageText, option.Rectangle);

            Image = image;

            Sound.Play($"Answer_Incorrect_{Question.Difficulty}.wav");
        }

        public void ClickCorrect() => OnOptionClick(buttons[Question.Correct], EventArgs.Empty);

        public async Task Clear()
        {
            if (iconHint.Visible)
            {
                g.DrawImage(iconHint.BackgroundImage, new Rectangle(iconHint.Location, iconHint.Size));
                Image = image;

                iconHint.Visible = false;
                iconHint.Clear();
            }

            using (Image frame = new Bitmap(image))
                for (int i = CountFramesAlphaChange - 1; i > 0; i--)
                {
                    g.Clear(Color.Transparent);
                    DrawFrame(frame, ClientRectangle, i, CountFramesAlphaChange);

                    Image = image;
                    await Task.Delay(MainForm.DeltaTime);
                }

            g.Clear(Color.Transparent);
            Image = image;

            Reset();
        }

        public async Task ShowCorrect(bool playSound, bool addDelay, bool isSaveSum = false)
        {
            if (playSound)
                if (isSaveSum && IsCorrectAnswer && Question.Difficulty != DifficultyQuestion.Final)
                    Sound.Play("Answer_Correct_SaveSum.wav");
                else if (IsCorrectAnswer)
                    Sound.Play($"Answer_Correct_{Question.Difficulty}.wav");
                else
                    Sound.Play($"Answer_Incorrect_{Question.Difficulty}.wav");

            Option option = options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (Image startFrame = ResourceManager.GetImage(option.Selected ? "ButtonWire_Orange.png" : "ButtonWire_Blue.png"))
            using (Image finalFrame = ResourceManager.GetImage("ButtonWire_Green.png"))
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
            iconHint.BringToFront();
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

        public void SetSettings(GameSettingsData data) =>
            isSequentially = Convert.ToBoolean(data.GetSettings(GameSettings.ShowOptionsSequentially));
    }
}