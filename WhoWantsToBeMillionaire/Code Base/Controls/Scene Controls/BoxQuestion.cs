using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class BoxQuestion : GameContol, IResettable, ISettable
    {
        private const int AlphaChangeFramesCount = 6;

        private readonly Graphics _g;
        private readonly Image _image;
        private readonly ImageAlpha _wires;
        private readonly ImageAlphaText _textQuestion;
        private readonly CentralIconHint _centralIconHint;
        private readonly List<ImageAlphaText> _imageAlphaTexts;
        private readonly Dictionary<LetterOptions, Option> _options;
        private readonly Dictionary<LetterOptions, ButtonOption> _buttons;
        private readonly ReadOnlyDictionary<ButtonWireThemes, Image> _themeImagesOptions;

        private bool _isSequentially = true;

        public BoxQuestion(int width, int height) : base(width, height)
        {
            var questionRectangle = new Rectangle(0, 0, width, (int)(0.11f * width));
            var optionSize = new Size((int)(0.45f * width), questionRectangle.Height >> 1);
            var optionRectangle = new Rectangle(new Point(), optionSize);
            var lettersOption = CollectionFactory.GetEnum<LetterOptions>();
            var dy = (int)(0.1f * optionSize.Height);
            var background = new Bitmap(width, height);
            var iconHeight = optionSize.Height + dy;
            Option option;

            _image = new Bitmap(width, height);
            _options = new Dictionary<LetterOptions, Option>();
            _centralIconHint = new CentralIconHint();
            _textQuestion = new ImageAlphaText(questionRectangle);
            _imageAlphaTexts = new List<ImageAlphaText>();
            _themeImagesOptions = SpritePainter.GetEnumSpritesList<ButtonWireThemes>(Resources.ButtonWire);
            _g = Graphics.FromImage(_image);

            _centralIconHint.Size = Resizer.Resize(BasicSizes.Height, iconHeight, 8, 5);
            _centralIconHint.Location = new Point(width - _centralIconHint.Width >> 1, questionRectangle.Height + dy + (optionSize.Height >> 1));
            _centralIconHint.Visible = false;

            _textQuestion.Font = FontFactory.CreateFont(GameFonts.Arial, 0.45f * optionSize.Height);
            _textQuestion.LineLength = 64;

            for (int i = 0; i < lettersOption.Length; i++)
            {
                optionRectangle.X = (Width >> 1) - (i & 1 ^ 1) * optionSize.Width;
                optionRectangle.Y = questionRectangle.Height + (i >> 1) * (optionSize.Height + dy) + dy;

                option = new Option(lettersOption[i], optionRectangle);
                option.Font = FontFactory.CreateFont(GameFonts.Arial, 0.4f * optionSize.Height);

                _options.Add(option.Letter, option);
            }

            _imageAlphaTexts.Add(_textQuestion);
            _imageAlphaTexts = _imageAlphaTexts.Concat(_options.Values).ToList();

            _buttons = _options.ToDictionary(pair => pair.Key, pair => new ButtonOption(pair.Key, pair.Value.PositionRectangle));

            foreach (var button in _buttons.Values)
            {
                Controls.Add(button);
                button.Click += OnOptionClick;
            }

            using (var wires = new Bitmap(width, height))
            using (var gWire = Graphics.FromImage(wires))
            using (var gBack = Graphics.FromImage(background))
            using (var wire = Resources.Wire)
            using (var questionBack = Resources.Question)
            {
                gBack.DrawImage(questionBack, questionRectangle);

                foreach (var op in _options.Values)
                    gBack.DrawImage(_themeImagesOptions[ButtonWireThemes.Blue], op.PositionRectangle);

                foreach (var yWire in _options.Values.Select(op => op.PositionRectangle.Y).Distinct())
                    gWire.DrawImage(wire, 0, yWire, wires.Width, optionSize.Height);

                foreach (var op in _options.Values)
                    gWire.DrawImage(_themeImagesOptions[ButtonWireThemes.Blue], op.PositionRectangle);

                _wires = new ImageAlpha(wires);
            }

            BackgroundImage = background;

            Controls.Add(_centralIconHint);
        }

        public event Action<LetterOptions> OptionClick;

        public AnswerModes AnswerMode { get; set; }

        public Question Question { get; private set; }

        public bool IsCorrectAnswer { get; private set; }

        public async Task ShowQuestion()
        {
            _g.Clear(Color.Transparent);

            var alphas = GetAlphas();

            foreach (var alpha in alphas)
            {
                _wires.Alpha = alpha;
                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }

            if (_isSequentially)
            {
                int delay = 250 + 500 * (int)Question.Difficulty;

                foreach (var text in _imageAlphaTexts)
                {
                    foreach (var alpha in alphas)
                    {
                        text.Alpha = alpha;
                        _g.DrawImage(text.ImageText, text.PositionRectangle);

                        Invalidate();
                        await Task.Delay(GameConst.DeltaTime);
                    }

                    await Task.Delay(delay);
                }
            }
            else
            {
                foreach (var alpha in alphas)
                {
                    foreach (var text in _imageAlphaTexts)
                    {
                        text.Alpha = alpha;
                        _g.DrawImage(text.ImageText, text.PositionRectangle);
                    }

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }
        }

        public async Task Clear()
        {
            if (_centralIconHint.Visible)
            {
                _g.DrawImage(_centralIconHint.BackgroundImage, new Rectangle(_centralIconHint.Location, _centralIconHint.Size));

                _centralIconHint.Visible = false;
                _centralIconHint.Clear();
            }

            using (var frame = new ImageAlpha(_image))
            {
                foreach (var alpha in GetAlphas().Reverse())
                {
                    _wires.Alpha = frame.Alpha = alpha;

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(frame.Image, ClientRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }

            Reset();
        }

        public async Task ShowCorrect(bool playSound, bool addDelay, bool isSavingSum = false)
        {
            if (playSound)
            { 
                if (isSavingSum && IsCorrectAnswer && Question.Difficulty != QuestionDifficulties.Final)
                    Sound.Play(Resources.Answer_Correct_SavingSum);
                else if (IsCorrectAnswer)
                    Sound.Play($"Answer_Correct_{Question.Difficulty}");
                else
                    Sound.Play($"Answer_Incorrect_{Question.Difficulty}");
            }

            var option = _options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (var front = new ImageAlpha(_themeImagesOptions[ButtonWireThemes.Green]))
            {
                var back = _themeImagesOptions[option.IsSelected ? ButtonWireThemes.Orange : ButtonWireThemes.Blue];
                var alphasUp = GetAlphas();
                var alphasDown = alphasUp.Reverse();

                for (int stage = 0; stage < 5; stage++)
                {
                    foreach (var alpha in (stage & 1) == 0 ? alphasUp : alphasDown)
                    {
                        front.Alpha = alpha;

                        _g.DrawImage(back, option.PositionRectangle);
                        _g.DrawImage(front.Image, option.PositionRectangle);
                        _g.DrawImage(option.ImageText, option.PositionRectangle);

                        Invalidate();
                        await Task.Delay(GameConst.DeltaTime);
                    }
                }
            }

            if (addDelay)
                await Task.Delay(3000);
        }

        public async Task ShowCentralIcon(HintTypes hint, bool playSound)
        {
            _centralIconHint.Visible = true;
            _centralIconHint.BringToFront();

            await _centralIconHint.ShowIcon(hint, playSound);
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            Enabled = false;
            AnswerMode = AnswerModes.Default;

            _centralIconHint.Reset();
            _imageAlphaTexts.ForEach(text => text.Reset());
            _wires.Alpha = 0;
            _g.Clear(Color.Transparent);

            Invalidate();
        }

        public void SetSettings(SettingsData data)
        {
            _isSequentially = Convert.ToBoolean(data.GetSettings(GameSettings.ShowOptionsSequentially));
        }

        public void SetQuestion(int number)
        {
            SetQuestion(new Question(number));
        }

        public void SetQuestion(int number, int index)
        {
            SetQuestion(new Question(number, index));
        }

        public void SetQuestion(Question question)
        {
            Question = question;

            _g.Clear(Color.Transparent);

            _textQuestion.Text = question.Text;
            _g.DrawImage(_textQuestion.ImageText, _textQuestion.PositionRectangle);

            foreach (var option in _options.Values)
            {
                option.Text = question.Options[option.Letter];

                _buttons[option.Letter].Visible = option.Text != string.Empty;
                _g.DrawImage(option.ImageText, option.PositionRectangle);
            }

            Invalidate();
        }

        public void LockOption(LetterOptions letter)
        {
            var option = _options[letter];

            option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
            option.IsSelected = _buttons[option.Letter].Visible = false;

            _g.DrawImage(_themeImagesOptions[ButtonWireThemes.Gray], option.PositionRectangle);
            _g.DrawImage(option.ImageText, option.PositionRectangle);

            Invalidate();

            Sound.Play($"Answer_Incorrect_{Question.Difficulty}");
        }

        public void ClickCorrect()
        {
            OnOptionClick(_buttons[Question.Correct], EventArgs.Empty);
        }

        public async Task HideCentralIcon(bool playSound)
        {
            await _centralIconHint.HideIcon(playSound);
            _centralIconHint.Visible = _centralIconHint.BackgroundImage != null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            e.Graphics.DrawImage(_wires.Image, ClientRectangle);
            e.Graphics.DrawImage(_image, ClientRectangle);
        }

        private void OnOptionClick(object sender, EventArgs e)
        {
            if (sender is ButtonOption option)
            {
                Enabled = false;

                SelectOption(option.Letter);

                if (Question.Difficulty > QuestionDifficulties.Easy && AnswerMode < AnswerModes.SwitchQuestion)
                {
                    Sound.Play(Resources.Answer_Accepted);
                    Music.Play(Resources.Answer_DrumRoll);
                }

                OptionClick?.Invoke(option.Letter);
            }
        }

        private async void SelectOption(LetterOptions letter)
        {
            IsCorrectAnswer = letter == Question.Correct;

            var option = _options[letter];

            option.IsSelected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (var selectedOption = new ImageAlpha(_themeImagesOptions[ButtonWireThemes.Orange]))
            {
                foreach (var alpha in GetAlphas())
                {
                    selectedOption.Alpha = alpha;

                    _g.DrawImage(selectedOption.Image, option.PositionRectangle);
                    _g.DrawImage(option.ImageText, option.PositionRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }
        }

        private IEnumerable<int> GetAlphas()
        {
            return Enumerable.Range(0, AlphaChangeFramesCount).Select(frameIndex => byte.MaxValue * frameIndex / (AlphaChangeFramesCount - 1));
        }
    }
}