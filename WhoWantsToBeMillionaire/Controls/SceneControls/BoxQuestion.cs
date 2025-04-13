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
    class BoxQuestion : GameContol, IReset, ISetSettings
    {
        private const int CountFramesAlphaChange = 6;

        private readonly Graphics _g;
        private readonly Image _image;
        private readonly ImageAlpha _wires;
        private readonly ImageAlphaText _textQuestion;
        private readonly CentralIconHint _centralIconHint;
        private readonly List<ImageAlphaText> _imageAlphaTexts;
        private readonly Dictionary<LetterOption, Option> _options;
        private readonly Dictionary<LetterOption, ButtonOption> _buttons;
        private readonly ReadOnlyDictionary<ThemeButtonWire, Image> _themeImagesOptions;

        private bool _isSequentially = true;

        public Action<LetterOption> OptionClick;

        public BoxQuestion(int width, int height) : base(width, height)
        {
            var questionRectangle = new Rectangle(0, 0, width, (int)(0.11f * width));
            var optionSize = new Size((int)(0.45f * width), questionRectangle.Height >> 1);
            var optionRectangle = new Rectangle(new Point(), optionSize);
            var lettersOption = Question.LettersOption.ToArray();
            var dy = (int)(0.1f * optionSize.Height);
            var background = new Bitmap(width, height);
            var iconHeight = optionSize.Height + dy;
            Option option;

            _image = new Bitmap(width, height);
            _options = new Dictionary<LetterOption, Option>();
            _centralIconHint = new CentralIconHint();
            _textQuestion = new ImageAlphaText(questionRectangle);
            _imageAlphaTexts = new List<ImageAlphaText>();
            _themeImagesOptions = Painter.GetThemeImages<ThemeButtonWire>(Resources.ButtonWire);
            _g = Graphics.FromImage(_image);

            _centralIconHint.Size = Resizer.Resize(BasicSize.Height, iconHeight, 8, 5);
            _centralIconHint.Location = new Point(width - _centralIconHint.Width >> 1, questionRectangle.Height + dy + (optionSize.Height >> 1));
            _centralIconHint.Visible = false;

            _textQuestion.Font = FontManager.CreateFont(GameFont.Arial, 0.45f * optionSize.Height);
            _textQuestion.LengthLine = 64;

            for (int i = 0; i < lettersOption.Length; i++)
            {
                optionRectangle.X = (Width >> 1) - (i & 1 ^ 1) * optionSize.Width;
                optionRectangle.Y = questionRectangle.Height + (i >> 1) * (optionSize.Height + dy) + dy;

                option = new Option(lettersOption[i], optionRectangle);
                option.Font = FontManager.CreateFont(GameFont.Arial, 0.4f * optionSize.Height);

                _options.Add(option.Letter, option);
            }

            _imageAlphaTexts.Add(_textQuestion);
            _imageAlphaTexts = _imageAlphaTexts.Concat(_options.Values).ToList();

            _buttons = _options.ToDictionary(k => k.Key, v => new ButtonOption(v.Key, v.Value.PositionRectangle));

            foreach (var b in _buttons.Values)
            {
                Controls.Add(b);
                b.Click += OnOptionClick;
            }

            using (var wires = new Bitmap(width, height))
            using (var gWire = Graphics.FromImage(wires))
            using (var gBack = Graphics.FromImage(background))
            using (var wire = Resources.Wire)
            using (var questionBack = Resources.Question)
            {
                gBack.DrawImage(questionBack, questionRectangle);

                foreach (var op in _options.Values)
                    gBack.DrawImage(_themeImagesOptions[ThemeButtonWire.Blue], op.PositionRectangle);

                foreach (var yWire in _options.Values.Select(t => t.PositionRectangle.Y).Distinct())
                    gWire.DrawImage(wire, 0, yWire, wires.Width, optionSize.Height);

                foreach (var op in _options.Values)
                    gWire.DrawImage(_themeImagesOptions[ThemeButtonWire.Blue], op.PositionRectangle);

                _wires = new ImageAlpha(wires);
            }

            BackgroundImage = background;

            Controls.Add(_centralIconHint);
        }

        public AnswerMode AnswerMode { get; set; }

        public Question Question { get; private set; }

        public bool IsCorrectAnswer { get; private set; }

        public void Reset(Mode mode = Mode.Classic)
        {
            Enabled = false;
            AnswerMode = AnswerMode.Default;

            _centralIconHint.Reset();
            _imageAlphaTexts.ForEach(t => t.Reset());
            _wires.Alpha = 0;
            _g.Clear(Color.Transparent);

            Invalidate();
        }

        public void SetSettings(SettingsData data) =>
            _isSequentially = Convert.ToBoolean(data.GetSettings(GameSettings.ShowOptionsSequentially));

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
            e.Graphics.DrawImage(_wires.Image, ClientRectangle);
            e.Graphics.DrawImage(_image, ClientRectangle);
        }

        private IEnumerable<int> GetAlphas() =>
            Enumerable.Range(0, CountFramesAlphaChange).Select(x => byte.MaxValue * x / (CountFramesAlphaChange - 1));

        public void SetQuestion(int number) =>
            SetQuestion(new Question(number));

        public void SetQuestion(int number, int index) =>
            SetQuestion(new Question(number, index));

        public void SetQuestion(Question question)
        {
            Question = question;

            _g.Clear(Color.Transparent);

            _textQuestion.Text = question.Text;
            _g.DrawImage(_textQuestion.ImageText, _textQuestion.PositionRectangle);

            foreach (var op in _options.Values)
            {
                op.Text = question.Options[op.Letter];

                _buttons[op.Letter].Visible = op.Text != string.Empty;
                _g.DrawImage(op.ImageText, op.PositionRectangle);
            }

            Invalidate();
        }

        public async Task ShowQuestion()
        {
            _g.Clear(Color.Transparent);

            var alphas = GetAlphas();

            foreach (var a in alphas)
            {
                _wires.Alpha = a;
                Invalidate();
                await Task.Delay(GameConst.DeltaTime);
            }

            if (_isSequentially)
            {
                int delay = 250 + 500 * (int)Question.Difficulty;

                foreach (var text in _imageAlphaTexts)
                {
                    foreach (var a in alphas)
                    {
                        text.Alpha = a;
                        _g.DrawImage(text.ImageText, text.PositionRectangle);

                        Invalidate();
                        await Task.Delay(GameConst.DeltaTime);
                    }

                    await Task.Delay(delay);
                }
            }
            else
            {
                foreach (var a in alphas)
                {
                    foreach (var text in _imageAlphaTexts)
                    {
                        text.Alpha = a;
                        _g.DrawImage(text.ImageText, text.PositionRectangle);
                    }

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }
        }

        private void OnOptionClick(object sender, EventArgs e)
        {
            if (sender is ButtonOption option)
            {
                Enabled = false;

                SelectOption(option.Letter);

                if (Question.Difficulty > DifficultyQuestion.Easy && AnswerMode < AnswerMode.SwitchQuestion)
                {
                    GameSound.Play(Resources.Answer_Accepted);
                    GameMusic.Play(Resources.Answer_DrumRoll);
                }

                OptionClick?.Invoke(option.Letter);
            }
        }

        private async void SelectOption(LetterOption letter)
        {
            IsCorrectAnswer = letter == Question.Correct;

            var option = _options[letter];

            option.Selected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (var selectedOption = new ImageAlpha(_themeImagesOptions[ThemeButtonWire.Orange]))
                foreach (var a in GetAlphas())
                {
                    selectedOption.Alpha = a;

                    _g.DrawImage(selectedOption.Image, option.PositionRectangle);
                    _g.DrawImage(option.ImageText, option.PositionRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
        }

        public void LockOption(LetterOption letter)
        {
            var option = _options[letter];

            option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
            option.Selected = _buttons[option.Letter].Visible = false;

            _g.DrawImage(_themeImagesOptions[ThemeButtonWire.Gray], option.PositionRectangle);
            _g.DrawImage(option.ImageText, option.PositionRectangle);

            Invalidate();

            GameSound.Play($"Answer_Incorrect_{Question.Difficulty}");
        }

        public void ClickCorrect() =>
            OnOptionClick(_buttons[Question.Correct], EventArgs.Empty);

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
                foreach (var a in GetAlphas().Reverse())
                {
                    _wires.Alpha = frame.Alpha = a;

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(frame.Image, ClientRectangle);

                    Invalidate();
                    await Task.Delay(GameConst.DeltaTime);
                }
            }

            Reset();
        }

        public async Task ShowCorrect(bool playSound, bool addDelay, bool isSaveSum = false)
        {
            if (playSound)
                if (isSaveSum && IsCorrectAnswer && Question.Difficulty != DifficultyQuestion.Final)
                    GameSound.Play(Resources.Answer_Correct_SaveSum);
                else if (IsCorrectAnswer)
                    GameSound.Play($"Answer_Correct_{Question.Difficulty}");
                else
                    GameSound.Play($"Answer_Incorrect_{Question.Difficulty}");

            var option = _options[Question.Correct];

            option.SetForeColors(Color.White, Color.Black);

            using (var front = new ImageAlpha(_themeImagesOptions[ThemeButtonWire.Green]))
            {
                var back = _themeImagesOptions[option.Selected ? ThemeButtonWire.Orange : ThemeButtonWire.Blue];
                var alphasUp = GetAlphas();
                var alphasDown = alphasUp.Reverse();

                for (int stage = 0; stage < 5; stage++)
                    foreach (var a in (stage & 1) == 0 ? alphasUp : alphasDown)
                    {
                        front.Alpha = a;

                        _g.DrawImage(back, option.PositionRectangle);
                        _g.DrawImage(front.Image, option.PositionRectangle);
                        _g.DrawImage(option.ImageText, option.PositionRectangle);

                        Invalidate();
                        await Task.Delay(GameConst.DeltaTime);
                    }
            }

            if (addDelay)
                await Task.Delay(3000);
        }

        public async Task ShowCentralIcon(TypeHint hint, bool playSound)
        {
            _centralIconHint.Visible = true;
            _centralIconHint.BringToFront();

            await _centralIconHint.ShowIcon(hint, playSound);
        }

        public async Task HideCentralIcon(bool playSound)
        {
            await _centralIconHint.HideIcon(playSound);
            _centralIconHint.Visible = _centralIconHint.BackgroundImage != null;
        }
    }
}