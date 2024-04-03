using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum SceneCommand
    {
        Start,
        Restart,
        Show_SaveSums,
        Show_CountHints,
        Show_Hint,
        About_RestrictionsHints,
        About_TakingMoney,
        About_Starting,
        ChoosingSaveSum,
        NextQuestion,
        Loss,
        Victory,
        End_PhoneFriend,
        End_AskAudience,
        End_AskHost,
        SwitchQuestion,
        TakeMoney,
        TakeMoney_Confirmation,
        TakeMoney_ShowPrize,
        FinalQuestion
    }

    enum SceneCancelCommand
    {
        SkipRules,
        CancelTakingMoney,
        ExitToMainMenu
    }

    class Scene : GameContol, IReset, IGameSettings
    {
        private readonly BoxAnimation _boxAnimation;
        private readonly BoxQuestion _boxQuestion;
        private readonly ButtonСapsule _buttonTakeMoney;
        private readonly CommandBoard _commandBoard;
        private readonly Host _host;
        private readonly Hint _hint;
        private readonly Image _prizeImage;
        private readonly MovingTableControls _tableControls;
        private readonly TableHints _tableHints;
        private readonly TableSums _tableSums;

        private PhoneTimer _timer;
        private VotingChart _chart;

        public delegate void EventGameOver(bool isRestart);
        public delegate void EventAchievementСompleted(Achievement achievement);
        public delegate void EventStatisticsChanged(StatsAttribute attribute, int value = 1);

        public event EventGameOver GameOver;
        public event EventStatisticsChanged StatisticsChanged;
        public event EventAchievementСompleted AchievementСompleted;

        public Mode Mode { private set; get; } = Mode.Classic;

        public bool MenuAllowed { private set; get; } = false;

        private bool ControlEnabled
        {
            set
            {
                _buttonTakeMoney.Enabled = value;
                _boxQuestion.Enabled = value;
                _tableHints.Enabled = value;
            }
        }

        private bool QuestionVisible
        {
            set
            {
                _boxQuestion.Visible = value;
                _boxAnimation.Visible = !value;
            }
        }

        public Scene() : base(MainForm.ScreenRectangle.Size)
        {
            Dock = DockStyle.Fill;

            _host = new Host();
            _tableSums = new TableSums();
            _hint = new Hint();
            _tableControls = new MovingTableControls((int)(MainForm.ScreenRectangle.Width * 0.3f), MainForm.ScreenRectangle.Height);
            _boxAnimation = new BoxAnimation(MainForm.ScreenRectangle.Width - _tableControls.Width, (int)(MainForm.ScreenRectangle.Height * 0.36f));
            _boxQuestion = new BoxQuestion(_boxAnimation.Width, _boxAnimation.Height);
            _buttonTakeMoney = new ButtonСapsule((int)(0.8f * _tableControls.Width), (int)(0.05f * _tableControls.Height));
            _commandBoard = new CommandBoard(MainForm.ScreenRectangle.Width - _tableControls.Width, MainForm.ScreenRectangle.Height - _boxQuestion.Height);
            _tableHints = new TableHints(_tableControls.Width, (int)(_tableControls.Height * 0.2f));
            _prizeImage = new Bitmap(_boxAnimation.Width, _boxAnimation.Height);

            _boxAnimation.Location = _boxQuestion.Location = new Point(0, MainForm.ScreenRectangle.Height - _boxQuestion.Height);
            _buttonTakeMoney.Text = "Забрать деньги";

            using (Graphics g = Graphics.FromImage(_prizeImage))
            using (Image img = Resources.Question)
            {
                int height = _prizeImage.Width * img.Height / img.Width;
                int y = (_prizeImage.Height - height) >> 1;
                _boxAnimation.SizeFont = 0.6f * height;

                g.DrawImage(img, 0, y, _prizeImage.Width, height);
            }

            _commandBoard.CommandClick += OnCommandClick;
            _commandBoard.CancelClick += OnCacnelClick;
            _buttonTakeMoney.Click += OnTakeMoneyClick;
            _tableHints.HintClick += OnHintClick;
            _boxQuestion.OptionClick += OnOptionClick;

            _tableControls.Add(_tableHints, 20f, 1f, 1f);
            _tableControls.Add(_tableSums, 67f, 0.8f, 1f);
            _tableControls.Add(_buttonTakeMoney, 13f, 0.7f, 0.4f);

            Controls.Add(_tableControls);
            Controls.Add(_commandBoard);
            Controls.Add(_boxAnimation);
            Controls.Add(_boxQuestion);
        }

        public void Reset(Mode mode)
        {
            Mode = mode;

            foreach (Control ctrl in Controls)
                if (ctrl is IReset)
                    (ctrl as IReset).Reset(mode);

            QuestionVisible = MenuAllowed = false;
        }

        public async void Start()
        {
            MenuAllowed = true;

            Sound.PlayBackground(Resources.Rules);

            _commandBoard.Command = Mode == Mode.Classic ? SceneCommand.Show_SaveSums : SceneCommand.Show_CountHints;
            _commandBoard.CancelCommand = SceneCancelCommand.SkipRules;

            await _tableControls.MoveX(MainForm.ScreenRectangle.Width - _tableControls.Width, 600 / MainForm.DeltaTime);

            _tableSums.Visible = _tableHints.Visible = true;
            _commandBoard.ButtonCommandEnabled = false;
            _commandBoard.Text = _host.Say(HostPhrases.Rules, Question.MaxNumber.ToString()); ;

            await Task.Delay(1000);

            _commandBoard.ButtonsVisible = true;

            await _tableSums.ShowSums();

            _commandBoard.ButtonCommandEnabled = true;
        }

        public async void Restart()
        {
            MenuAllowed = true;

            Sound.PlayBackground(Resources.Rules);

            await _tableControls.MoveX(MainForm.ScreenRectangle.Width - _tableControls.Width, 600 / MainForm.DeltaTime);

            _tableSums.Visible = _tableHints.Visible = true;
            _tableHints.ShowAllHints();

            _commandBoard.ButtonCommandVisible = true;

            OnCommandClick(this, Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
        }

        private async void OnOptionClick(Letter letter)
        {
            string explanation = _boxQuestion.Question.Explanation;
            if (!_boxQuestion.IsCorrectAnswer)
                explanation += $"\nПравильный ответ: {_boxQuestion.Question.FullCorrect}.";

            switch (_boxQuestion.AnswerMode)
            {
                default:
                    ControlEnabled = false;
                    StatisticsChanged.Invoke(_boxQuestion.IsCorrectAnswer ? StatsAttribute.NumberCorrectAnswers : StatsAttribute.NumberIncorrectAnswers);

                    if (_boxQuestion.IsCorrectAnswer && _boxQuestion.Question.Number < Question.MaxNumber)
                    {
                        _commandBoard.Command = SceneCommand.NextQuestion;
                    }
                    else
                    {
                        _buttonTakeMoney.Visible = false;
                        _commandBoard.Command = _boxQuestion.IsCorrectAnswer ? SceneCommand.Victory : SceneCommand.Loss;
                    }

                    _commandBoard.Text = explanation;
                    break;

                case AnswerMode.DoubleDips:
                    if (!_boxQuestion.IsCorrectAnswer)
                    {
                        _boxQuestion.AnswerMode = AnswerMode.Usual;
                        await Task.Delay(3000);

                        Sound.StopAll();
                        _boxQuestion.LockOption(letter);

                        AchievementСompleted.Invoke(Achievement.SuccessfulOutcome);

                        if (_boxQuestion.Question.CountOptions == 2)
                        {
                            await Task.Delay(3000);
                            _boxQuestion.ClickCorrect();
                        }
                        else
                        {
                            _boxQuestion.PlayBackgroundSound(Resources.Hint_DoubleDip);
                            _boxQuestion.Enabled = true;
                            return;
                        }
                    }
                    else
                        goto default;
                    break;

                case AnswerMode.SwitchQuestion:
                    _commandBoard.Command = SceneCommand.SwitchQuestion;

                    HostPhrases phrase1 = _boxQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestion_CorrectAnswer : HostPhrases.SwitchQuestion_IncorrectAnswer;

                    _commandBoard.Text = $"{explanation}\n{_host.Say(phrase1, _boxQuestion.Question.Number.ToString())}";
                    break;

                case AnswerMode.TakeMoney:
                    _commandBoard.Command = SceneCommand.TakeMoney_ShowPrize;

                    HostPhrases phrase2 = _boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    _commandBoard.Text = $"{explanation}\n{_host.Say(phrase2, _tableSums.NextSum.ToString())}";
                    break;
            }

            _commandBoard.ButtonCommandVisible = true;
        }

        private async void OnHintClick(TypeHint type)
        {
            StatisticsChanged.Invoke(StatsAttribute.NumberHintsUsed);

            _buttonTakeMoney.Enabled = _tableHints.Enabled = type == TypeHint.FiftyFifty;

            switch (type)
            {
                case TypeHint.FiftyFifty:
                    Sound.Play(Resources.Hint_FiftyFifty);
                    _boxQuestion.SetQuestion(_hint.ReduceOptions(_boxQuestion.Question));
                    AchievementСompleted.Invoke(Achievement.DearComputer);
                    break;

                case TypeHint.PhoneFriend:
                    Sound.PlayBackground(Resources.Hint_PhoneFriend_Dialing);

                    _boxQuestion.Enabled = false;
                    _commandBoard.TextMode = TextMode.Dialog;
                    _commandBoard.Command = SceneCommand.End_PhoneFriend;

                    _timer = new PhoneTimer((int)(0.3f * _commandBoard.Height));
                    _timer.TimeUp += OnCommandClick;

                    await Task.Delay(2000);

                    Sound.Play(Resources.Hint_PhoneFriend_Beeps);

                    await Task.Delay(6000);

                    foreach (var phrase in _hint.PhoneFriendDialog(_tableSums.NextSum))
                    {
                        _commandBoard.AddText(phrase);
                        await Task.Delay(phrase.Length * 75);
                    }

                    await _commandBoard.ShowMovingPictureBox(_timer, 500, false);

                    _commandBoard.Text = _hint.PhoneFriendAnswer(_boxQuestion.Question);
                    _timer.Start();

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.AskAudience:
                    Sound.PlayBackground(Resources.Hint_AskAudience_Begin);

                    _boxQuestion.Enabled = false;
                    _commandBoard.Command = SceneCommand.End_AskAudience;

                    int heigth = (int)(0.7f * _commandBoard.Height);
                    _chart = new VotingChart((int)(0.75f * heigth), heigth);

                    await _commandBoard.ShowMovingPictureBox(_chart, 500, true);
                    await Task.Delay(3000);
                    await _chart.ShowAnimationVote(3000);
                    await _chart.ShowPercents(_hint.PercentsAudience(_boxQuestion.Question), 15);

                    AchievementСompleted.Invoke(Achievement.AudienceAward);

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.DoubleDip:
                    _boxQuestion.AnswerMode = AnswerMode.DoubleDips;
                    _boxQuestion.PlayBackgroundSound(Resources.Hint_DoubleDip);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.SwitchQuestion:
                    _boxQuestion.AnswerMode = AnswerMode.SwitchQuestion;
                    _commandBoard.Text = _host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.AskHost:
                    _boxQuestion.Enabled = false;
                    _boxQuestion.PlayBackgroundSound(Resources.Hint_AskHost);

                    _commandBoard.Text = _hint.HostAnswer(_boxQuestion.Question);

                    await _boxQuestion.ShowCentralIcon(type, true);

                    _commandBoard.Command = SceneCommand.End_AskHost;
                    _commandBoard.ButtonCommandVisible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingControl box, int milliseconds)
        {
            await _commandBoard.RemoveMovingPictureBox(box, milliseconds / MainForm.DeltaTime);
            box.Dispose();
        }

        private void SaveSumSelected(int sum)
        {
            _commandBoard.Text =
                $"{_host.Say(HostPhrases.SaveSumSelected, String.Format("{0:#,0}", sum))}\n" +
                $"{_host.Say(HostPhrases.GameStart)}";

            _tableSums.SaveSumSelected -= SaveSumSelected;

            _commandBoard.Command = SceneCommand.Start;
            _commandBoard.ButtonCommandVisible = true;
        }

        private void OnTakeMoneyClick(object sender, EventArgs e)
        {
            ControlEnabled = false;
            _commandBoard.AskTakingMoney(_host.Say(HostPhrases.TakingMoney_ClarifyDecision));
        }

        private async Task ShowQuestion(int number)
        {
            QuestionVisible = false;
            _boxQuestion.SetQuestion(number);

            if (_boxQuestion.Question.Difficulty != DifficultyQuestion.Easy)
            {
                if (_boxQuestion.Question.Difficulty == DifficultyQuestion.Final)
                {
                    Sound.Play(Resources.Start);
                    await Task.Delay(5000);
                }
                else
                {
                    Sound.Play(Resources.Question_Next);
                    await Task.Delay(3000);
                }

                Sound.PlayBackground(Resources.Question_Reflections);
            }

            await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

            QuestionVisible = true;

            await _boxQuestion.ShowQuestion();

            ControlEnabled = true;
        }

        private async Task ShowCorrectAndPrize(bool playSound, bool addDelay, bool updatePrize)
        {
            _commandBoard.Clear();
            Sound.StopAll();

            await _boxQuestion.ShowCorrect(playSound, addDelay, _tableSums.NowSaveSum);

            if (_boxQuestion.Question.Number == 5 && !_tableSums.NowSaveSum)
                Sound.Play(Resources.Answer_Correct_Easy_Ending);

            if (updatePrize)
                _tableSums.Update(_boxQuestion.IsCorrectAnswer);

            await _boxQuestion.Clear();
            await Task.Delay(500);

            QuestionVisible = false;

            await _boxAnimation.ShowTransition(_boxQuestion.BackgroundImage, _prizeImage);
            await _boxAnimation.ShowText(_tableSums.TextPrize);
        }

        private async void OnCommandClick(object sender, SceneCommand command)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    int delay;

                    if (!_tableSums.NowSaveSum && _boxQuestion.Question.Number == 5)
                        delay = 3500;
                    else
                        delay = _tableSums.NowSaveSum ? 7000 : 1500 + 500 * (int)_boxQuestion.Question.Difficulty;

                    if (_tableSums.NowSaveSum)
                        AchievementСompleted.Invoke(Achievement.MoneyNotBurn);

                    await ShowCorrectAndPrize(true, false, true);
                    await Task.Delay(delay);
                    await _boxAnimation.HideImage();

                    if (_boxQuestion.Question.Number + 1 < Question.MaxNumber)
                    {
                        await Task.Delay(1000);
                        await ShowQuestion(_boxQuestion.Question.Number + 1);
                    }
                    else
                    {
                        _commandBoard.Text = _host.Say(HostPhrases.AboutFinalQuestion, Question.MaxNumber.ToString(), _tableSums.NextSum);
                        _commandBoard.Command = SceneCommand.FinalQuestion;
                        _commandBoard.ButtonCommandVisible = true;
                    }
                    break;

                case SceneCommand.FinalQuestion:
                    _commandBoard.Clear();
                    await ShowQuestion(Question.MaxNumber);
                    break;

                case SceneCommand.Loss:
                case SceneCommand.Victory:
                    await ShowCorrectAndPrize(true, true, true);

                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, _tableSums.Prize);

                    if (command == SceneCommand.Victory)
                    {
                        AchievementСompleted.Invoke(Achievement.Millionaire);

                        if (_tableHints.CountUsedHints == 0)
                            AchievementСompleted.Invoke(Achievement.TriumphReason);

                        await Task.Delay(16000);
                    }

                    _commandBoard.AskRestart();
                    break;

                case SceneCommand.Show_SaveSums:
                    _commandBoard.ButtonCommandEnabled = false;
                    _commandBoard.Command = SceneCommand.Show_CountHints;
                    _commandBoard.Text = _host.Say(HostPhrases.SaveSums, string.Join(", ", _tableSums.SaveSums.Select(x => string.Format("{0:#,0}", x))));

                    await _tableSums.ShowSaveSums();

                    _commandBoard.ButtonCommandEnabled = true;
                    break;

                case SceneCommand.Show_CountHints:
                    _commandBoard.Command = SceneCommand.Show_Hint;
                    _commandBoard.Text = _host.Say(HostPhrases.CountHints, _tableHints.TextActiveHints);
                    break;

                case SceneCommand.Show_Hint:
                    _commandBoard.Text = _tableHints.DescriptionNextHint;
                    _tableHints.ShowHint();

                    if (_tableHints.AllHintsVisible)
                        _commandBoard.Command = _tableHints.CountHints > Hint.MaxCountAllowedHints ? SceneCommand.About_RestrictionsHints : SceneCommand.About_TakingMoney;
                    break;

                case SceneCommand.About_RestrictionsHints:
                    _commandBoard.Command = SceneCommand.About_TakingMoney;
                    _commandBoard.Text = _host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxCountAllowedHints.ToString());
                    break;

                case SceneCommand.About_TakingMoney:
                    _commandBoard.Command = Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum;
                    _commandBoard.Text = _host.Say(HostPhrases.AboutTakingMoney);
                    break;

                case SceneCommand.ChoosingSaveSum:
                    _commandBoard.ButtonsVisible = false;
                    _commandBoard.Text = _host.Say(HostPhrases.AskSaveSum);

                    _tableSums.SaveSumSelected += SaveSumSelected;
                    _tableSums.AddSelectionSaveSum();
                    break;

                case SceneCommand.About_Starting:
                    _commandBoard.ButtonCancelVisible = false;
                    _commandBoard.Command = SceneCommand.Start;
                    _commandBoard.Text = _host.Say(HostPhrases.GameStart);
                    break;

                case SceneCommand.Start:
                    _commandBoard.Clear();
                    _tableSums.Clear();

                    Sound.StopBackground();
                    Sound.Play(Resources.Start);

                    await Task.Delay(3000);
                    await ShowQuestion(1);

                    _buttonTakeMoney.Visible = true;
                    break;

                case SceneCommand.End_PhoneFriend:
                    _commandBoard.Clear();
                    _commandBoard.TextMode = TextMode.Monologue;

                    _timer.Stop();
                    _timer.TimeUp -= OnCommandClick;

                    if (sender is CommandBoard)
                    {
                        Sound.StopPeek();
                        Sound.Play(Resources.Hint_PhoneFriend_End);
                    }

                    AchievementСompleted.Invoke(Achievement.AndToTalk);

                    _boxQuestion.PlayBackgroundSound(Resources.Question_Reflections);

                    await Task.Delay(2000);
                    await RemoveMovingPictureBox(_timer, 500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskAudience:
                    _commandBoard.Clear();

                    _boxQuestion.PlayBackgroundSound(Resources.Question_Reflections);

                    await RemoveMovingPictureBox(_chart, 500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.SwitchQuestion:
                    _commandBoard.Clear();

                    int newIndex;

                    do
                        newIndex = Question.RandomIndex(_boxQuestion.Question.Number);
                    while (newIndex == _boxQuestion.Question.Index);

                    await _boxQuestion.ShowCorrect(false, true);
                    await Task.Delay(3000);
                    await _boxQuestion.Clear();

                    AchievementСompleted.Invoke(Achievement.DefectiveQuestion);

                    QuestionVisible = false;

                    await _boxAnimation.HideImage(_boxQuestion.BackgroundImage);
                    await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

                    if (_boxQuestion.Question.CountOptions == 2)
                        AchievementСompleted.Invoke(Achievement.WasTwoBecameFour);

                    QuestionVisible = true;
                    _boxQuestion.SetQuestion(_boxQuestion.Question.Number, newIndex);

                    await _boxQuestion.ShowCentralIcon(TypeHint.SwitchQuestion, false);
                    await _boxQuestion.ShowQuestion();

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskHost:
                    _commandBoard.Clear();

                    AchievementСompleted.Invoke(Achievement.NoOneWillKnow);

                    _boxQuestion.PlayBackgroundSound(Resources.Question_Reflections);

                    await _boxQuestion.HideCentralIcon(true);

                    ControlEnabled = true;
                    break;

                case SceneCommand.TakeMoney_Confirmation:
                    _commandBoard.Command = SceneCommand.TakeMoney;
                    _buttonTakeMoney.Visible = false;

                    var phrase = _tableSums.Prize > 0 ? HostPhrases.PlayerTakingMoney : HostPhrases.PlayerTakingMoney_Zero;

                    _commandBoard.Clear();
                    _commandBoard.Text = _host.Say(phrase, _tableSums.TextPrize);
                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case SceneCommand.TakeMoney:
                    _commandBoard.Clear();
                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, _tableSums.Prize);

                    Sound.StopAll();
                    Sound.Play(Resources.PlayerTakesMoney);

                    await Task.Delay(7000);

                    _commandBoard.Text = _host.Say(HostPhrases.TakingMoney_AskAnswer);
                    _boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    _boxQuestion.Enabled = true;
                    break;

                case SceneCommand.TakeMoney_ShowPrize:
                    await ShowCorrectAndPrize(false, true, false);

                    if (_tableSums.Prize == 0)
                        AchievementСompleted.Invoke(Achievement.IsPossible);

                    AchievementСompleted.Invoke(Achievement.StopGame);

                    _commandBoard.AskRestart();
                    break;

                case SceneCommand.Restart:
                    Sound.StopAll();
                    GameOver.Invoke(true);
                    break;
            }
        }

        private void OnCacnelClick(object sender, SceneCancelCommand command)
        {
            switch (command)
            {
                case SceneCancelCommand.SkipRules:
                    _tableSums.CancelTask();
                    _tableHints.ShowAllHints();

                    OnCommandClick(this, Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
                    break;

                case SceneCancelCommand.CancelTakingMoney:
                    _commandBoard.Clear();
                    ControlEnabled = true;
                    break;

                case SceneCancelCommand.ExitToMainMenu:
                    GameOver.Invoke(false);
                    break;
            }
        }

        public void SetSettings(GameSettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is IGameSettings)
                    (ctrl as IGameSettings).SetSettings(data);
        }
    }
}
