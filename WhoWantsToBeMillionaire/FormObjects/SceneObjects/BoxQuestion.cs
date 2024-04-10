using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum AnswerMode
    {
        Usual,
        DoubleDips,
        SwitchQuestion,
        TakeMoney
    }

    class BoxQuestion : GameContol, IReset, ISetSettings
    {
        private const int CountFramesAlphaChange = 6;

        private readonly Graphics _g;
        private readonly Image _image;
        private readonly Image _wires;
        private readonly TextBitmap _textQuestion;
        private readonly Dictionary<Letter, Option> _options;
        private readonly Dictionary<Letter, ButtonOption> _buttons;
        private readonly CentralIconHint _iconHint;
        private readonly List<TextBitmap> _bitmapTexts;

        private bool _isSequentially = true;

        public Action<Letter> OptionClick;

        public AnswerMode AnswerMode { set; get; }

        public Question Question { private set; get; }

        public bool IsCorrectAnswer { private set; get; }

        public BoxQuestion(int width, int height) : base(width, height)
        {
            Rectangle rectQuestion = new Rectangle(0, 0, width, (int)(0.11f * width));
            Size sizeOption = new Size((int)(0.45f * width), rectQuestion.Height >> 1);

            _image = new Bitmap(width, height);
            _wires = new Bitmap(width, height);
            _options = new Dictionary<Letter, Option>();
            _iconHint = new CentralIconHint();
            _bitmapTexts = new List<TextBitmap>();
            _textQuestion = new TextBitmap(rectQuestion);
            _g = Graphics.FromImage(_image);

            _textQuestion.SizeFont = 0.45f * sizeOption.Height;
            _textQuestion.LengthLine = 64;

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
                _options.Add(option.Letter, option);
            }

            _bitmapTexts.Add(_textQuestion);
            _bitmapTexts = _bitmapTexts.Concat(_options.Values).ToList();

            _buttons = _options.ToDictionary(k => k.Key, v => new ButtonOption(v.Key, v.Value.Rectangle));

            foreach (var b in _buttons.Values)
            {
                Controls.Add(b);
                b.Click += OnOptionClick;
            }

            Image background = new Bitmap(width, height);

            using (Graphics gBack = Graphics.FromImage(background))
            using (Graphics gWire = Graphics.FromImage(_wires))
            using (Image wire = Resources.Wire)
            using (Image questionBack = Resources.Question)
            using (Image optionBack = Resources.ButtonWire_Blue)
            {
                gBack.DrawImage(questionBack, rectQuestion);

                foreach (var op in _options.Values)
                    gBack.DrawImage(optionBack, op.Rectangle);

                BackgroundImage = background;

                foreach (var yWire in _options.Values.Select(t => t.Rectangle.Y).Distinct())
                    gWire.DrawImage(wire, 0, yWire, _wires.Width, sizeOption.Height);

                foreach (var op in _options.Values)
                    gWire.DrawImage(optionBack, op.Rectangle);
            }

            int iconHeight = sizeOption.Height + dy;

            _iconHint.Size = new Size((int)(1.6f * iconHeight), iconHeight);
            _iconHint.Location = new Point((width - _iconHint.Width) >> 1, rectQuestion.Height + dy + (sizeOption.Height >> 1));
            _iconHint.Visible = false;

            Controls.Add(_iconHint);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            _iconHint.Reset();
            _bitmapTexts.ForEach(t => t.Reset());
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
                _g.DrawImage(frame, rectangle, 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel, attribute);
            }
        }

        public void SetQuestion(int number) =>
            SetQuestion(new Question(number));

        public void SetQuestion(int number, int index) =>
            SetQuestion(new Question(number, index));

        public void SetQuestion(Question question)
        {
            Question = question;

            _g.Clear(Color.Transparent);
            _g.DrawImage(_wires, 0, 0, Width, Height);

            _textQuestion.Text = question.Text;
            _g.DrawImage(_textQuestion.ImageText, _textQuestion.Rectangle);

            foreach (var op in _options.Values)
            {
                op.Text = question.Options[op.Letter];
                op.Enabled = op.Text != string.Empty;

                _buttons[op.Letter].Visible = op.Enabled;
                _g.DrawImage(op.ImageText, op.Rectangle);
            }

            Image = _image;
        }

        public async Task ShowQuestion()
        {
            Rectangle rectWires = new Rectangle(0, 0, Width, Height);

            for (int i = 1; i <= CountFramesAlphaChange; i++)
            {
                _g.Clear(Color.Transparent);
                DrawFrame(_wires, rectWires, i, CountFramesAlphaChange);

                Image = _image;
                await Task.Delay(MainForm.DeltaTime);
            }

            var alphas = Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

            if (_isSequentially)
            {
                int delay = 250 + 500 * (int)Question.Difficulty;

                foreach (var text in _bitmapTexts)
                {
                    foreach (var a in alphas)
                    {
                        text.Alpha = a;
                        _g.DrawImage(text.ImageText, text.Rectangle);
                        Image = _image;
                        await Task.Delay(MainForm.DeltaTime);
                    }

                    await Task.Delay(delay);
                }
            }
            else
            {
                foreach (var a in alphas)
                {
                    foreach (var text in _bitmapTexts)
                    {
                        text.Alpha = a;
                        _g.DrawImage(text.ImageText, text.Rectangle);
                    }

                    Image = _image;
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
                Sound.Play(Resources.Answer_Accepted);
                Sound.PlayLooped(Resources.Answer_DrumRoll);
            }

            OptionClick.Invoke(letter);
        }

        private async void SelectOption(Letter letter)
        {
            IsCorrectAnswer = letter == Question.Correct;

            Option option = _options[letter];

            option.Selected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (Image selectedOption = Resources.ButtonWire_Orange)
            {
                for (int i = 1; i <= CountFramesAlphaChange; i++)
                {
                    DrawFrame(selectedOption, option.Rectangle, i, CountFramesAlphaChange);
                    _g.DrawImage(option.ImageText, option.Rectangle);

                    Image = _image;
                    await Task.Delay(MainForm.DeltaTime);
                }
            }
        }

        public void LockOption(Letter letter)
        {
            Option option = _options[letter];

            option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
            option.Selected = option.Enabled = false;

            using (Image lockedOption = Resources.ButtonWire_Gray)
                _g.DrawImage(lockedOption, option.Rectangle);

            _g.DrawImage(option.ImageText, option.Rectangle);

            Image = _image;

            Sound.Play($"Answer_Incorrect_{Question.Difficulty}");
        }

        public void ClickCorrect() =>
            OnOptionClick(_buttons[Question.Correct], EventArgs.Empty);

        public async Task Clear()
        {
            if (_iconHint.Visible)
            {
                _g.DrawImage(_iconHint.BackgroundImage, new Rectangle(_iconHint.Location, _iconHint.Size));
                Image = _image;

                _iconHint.Visible = false;
                _iconHint.Clear();
            }

            using (Image frame = new Bitmap(_image))
                for (int i = CountFramesAlphaChange - 1; i > 0; i--)
                {
                    _g.Clear(Color.Transparent);
                    DrawFrame(frame, ClientRectangle, i, CountFramesAlphaChange);

                    Image = _image;
                    await Task.Delay(MainForm.DeltaTime);
                }

            _g.Clear(Color.Transparent);
            Image = _image;

            Reset();
        }

        public async Task ShowCorrect(bool playSound, bool addDelay, bool isSaveSum = false)
        {
            if (playSound)
                if (isSaveSum && IsCorrectAnswer && Question.Difficulty != DifficultyQuestion.Final)
                    Sound.Play(Resources.Answer_Correct_SaveSum);
                else if (IsCorrectAnswer)
                    Sound.Play($"Answer_Correct_{Question.Difficulty}");
                else
                    Sound.Play($"Answer_Incorrect_{Question.Difficulty}");

            Option option = _options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (Image startFrame = option.Selected ? Resources.ButtonWire_Orange : Resources.ButtonWire_Blue)
            using (Image finalFrame = Resources.ButtonWire_Green)
            {
                Image frame, back;

                for (int stage = 0; stage < 5; stage++)
                {
                    (frame, back) = (stage & 1) == 0 ? (finalFrame, startFrame) : (startFrame, finalFrame);

                    for (int i = 1; i <= CountFramesAlphaChange; i++)
                    {
                        _g.DrawImage(back, option.Rectangle);

                        DrawFrame(frame, option.Rectangle, i, CountFramesAlphaChange);

                        _g.DrawImage(option.ImageText, option.Rectangle);

                        Image = _image;
                        await Task.Delay(MainForm.DeltaTime);
                    }
                }
            }

            if (addDelay)
                await Task.Delay(3000);
        }

        public async Task ShowCentralIcon(TypeHint hint, bool playSound)
        {
            _iconHint.Visible = true;
            _iconHint.BringToFront();
            await _iconHint.ShowIcon(hint, playSound);
        }

        public async Task HideCentralIcon(bool playSound)
        {
            await _iconHint.HideIcon(playSound);
            _iconHint.Visible = _iconHint.BackgroundImage != null;
        }

        public void SetSettings(GameSettingsData data) =>
            _isSequentially = Convert.ToBoolean(data.GetSettings(GameSettings.ShowOptionsSequentially));
    }
}