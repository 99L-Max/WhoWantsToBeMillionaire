using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum AnswerMode { Usual, DoubleDips, SwitchQuestion, TakeMoney }

    class BoxQuestion : GameContol, IReset, ISetSettings
    {
        private const int CountFramesAlphaChange = 6;

        private readonly Graphics _g;
        private readonly Image _image;
        private readonly ImageAlpha _wires;
        private readonly ImageAlphaText _textQuestion;
        private readonly Dictionary<Letter, Option> _options;
        private readonly Dictionary<Letter, ButtonOption> _buttons;
        private readonly CentralIconHint _iconHint;
        private readonly List<ImageAlphaText> _imageAlphaTexts;

        private bool _isSequentially = true;

        public Action<Letter> OptionClick;

        public BoxQuestion(int width, int height) : base(width, height)
        {
            var questionRectangle = new Rectangle(0, 0, width, (int)(0.11f * width));
            var optionSize = new Size((int)(0.45f * width), questionRectangle.Height >> 1);
            var optionRectangle = new Rectangle(new Point(), optionSize);
            var keys = Question.Letters.ToArray();
            var dy = (int)(0.1f * optionSize.Height);
            var background = new Bitmap(width, height);
            var iconHeight = optionSize.Height + dy;

            Option option;

            _image = new Bitmap(width, height);
            _options = new Dictionary<Letter, Option>();
            _iconHint = new CentralIconHint();
            _imageAlphaTexts = new List<ImageAlphaText>();
            _textQuestion = new ImageAlphaText(questionRectangle);
            _g = Graphics.FromImage(_image);

            _iconHint.Size = new Size((int)(1.6f * iconHeight), iconHeight);
            _iconHint.Location = new Point((width - _iconHint.Width) >> 1, questionRectangle.Height + dy + (optionSize.Height >> 1));
            _iconHint.Visible = false;

            _textQuestion.Font = new Font("", 0.45f * optionSize.Height, GraphicsUnit.Pixel);
            _textQuestion.LengthLine = 64;

            for (int i = 0; i < keys.Length; i++)
            {
                optionRectangle.X = (Width >> 1) - (i & 1 ^ 1) * optionSize.Width;
                optionRectangle.Y = questionRectangle.Height + (i >> 1) * (optionSize.Height + dy) + dy;

                option = new Option(keys[i], optionRectangle);
                option.Font = new Font("", 0.4f * optionSize.Height, GraphicsUnit.Pixel);

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
            using (var optionBack = Resources.ButtonWire_Blue)
            {
                gBack.DrawImage(questionBack, questionRectangle);

                foreach (var op in _options.Values)
                    gBack.DrawImage(optionBack, op.PositionRectangle);

                foreach (var yWire in _options.Values.Select(t => t.PositionRectangle.Y).Distinct())
                    gWire.DrawImage(wire, 0, yWire, wires.Width, optionSize.Height);

                foreach (var op in _options.Values)
                    gWire.DrawImage(optionBack, op.PositionRectangle);

                _wires = new ImageAlpha(wires);
            }

            BackgroundImage = background;

            Controls.Add(_iconHint);
        }

        public AnswerMode AnswerMode { get; set; }

        public Question Question { get; private set; }

        public bool IsCorrectAnswer { get; private set; }

        public void Reset(Mode mode = Mode.Classic)
        {
            Enabled = false;
            AnswerMode = AnswerMode.Usual;

            _iconHint.Reset();
            _imageAlphaTexts.ForEach(t => t.Reset());
            _wires.Alpha = 0;
            _g.Clear(Color.Transparent);

            Invalidate();
        }

        public void SetSettings(GameSettingsData data) =>
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
                await Task.Delay(MainForm.DeltaTime);
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
                        await Task.Delay(MainForm.DeltaTime);
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
                    await Task.Delay(MainForm.DeltaTime);
                }
            }
        }

        private void OnOptionClick(object sender, EventArgs e)
        {
            if (sender is ButtonOption option)
            {
                Enabled = false;

                SelectOption(option.Letter);

                if (Question.Difficulty != DifficultyQuestion.Easy && (AnswerMode == AnswerMode.Usual || AnswerMode == AnswerMode.DoubleDips))
                {
                    Sound.Play(Resources.Answer_Accepted);
                    Sound.PlayLooped(Resources.Answer_DrumRoll);
                }

                OptionClick?.Invoke(option.Letter);
            }
        }

        private async void SelectOption(Letter letter)
        {
            IsCorrectAnswer = letter == Question.Correct;

            Option option = _options[letter];

            option.Selected = true;
            option.SetForeColors(Color.Black, Color.White);

            using (ImageAlpha selectedOption = new ImageAlpha(Resources.ButtonWire_Orange))
                foreach (var a in GetAlphas())
                {
                    selectedOption.Alpha = a;

                    _g.DrawImage(selectedOption.Image, option.PositionRectangle);
                    _g.DrawImage(option.ImageText, option.PositionRectangle);

                    Invalidate();
                    await Task.Delay(MainForm.DeltaTime);
                }
        }

        public void LockOption(Letter letter)
        {
            Option option = _options[letter];

            option.SetForeColors(Color.FromArgb(32, 32, 32), Color.DimGray);
            option.Selected = _buttons[option.Letter].Visible = false;

            using (Image lockedOption = Resources.ButtonWire_Gray)
                _g.DrawImage(lockedOption, option.PositionRectangle);

            _g.DrawImage(option.ImageText, option.PositionRectangle);

            Invalidate();

            Sound.Play($"Answer_Incorrect_{Question.Difficulty}");
        }

        public void ClickCorrect() =>
            OnOptionClick(_buttons[Question.Correct], EventArgs.Empty);

        public async Task Clear()
        {
            if (_iconHint.Visible)
            {
                _g.DrawImage(_iconHint.BackgroundImage, new Rectangle(_iconHint.Location, _iconHint.Size));

                _iconHint.Visible = false;
                _iconHint.Clear();
            }

            using (ImageAlpha frame = new ImageAlpha(_image))
            {
                foreach (var a in GetAlphas().Reverse())
                {
                    _wires.Alpha = frame.Alpha = a;

                    _g.Clear(Color.Transparent);
                    _g.DrawImage(frame.Image, ClientRectangle);

                    Invalidate();
                    await Task.Delay(MainForm.DeltaTime);
                }
            }

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

            using (var back = option.Selected ? Resources.ButtonWire_Orange : Resources.ButtonWire_Blue)
            using (var front = new ImageAlpha(Resources.ButtonWire_Green))
            {
                var alphasUp = GetAlphas();
                var alphasDown = alphasUp.Reverse();

                for (int stage = 0; stage < 5; stage++)
                {
                    foreach (var a in (stage & 1) == 0 ? alphasUp : alphasDown)
                    {
                        front.Alpha = a;

                        _g.DrawImage(back, option.PositionRectangle);
                        _g.DrawImage(front.Image, option.PositionRectangle);
                        _g.DrawImage(option.ImageText, option.PositionRectangle);

                        Invalidate();
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
    }
}