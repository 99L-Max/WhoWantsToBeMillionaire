using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Scene : GameContol, IResettable, ISettable
    {
        private readonly BoxAnimationTransition _boxAnimation;
        private readonly BoxQuestion _boxQuestion;
        private readonly ButtonСapsule _buttonTakeMoney;
        private readonly CommandBoard _commandBoard;
        private readonly Host _host;
        private readonly Image _prizeImage;
        private readonly MovingTableControls _tableControls;
        private readonly TableHints _tableHints;
        private readonly TableSums _tableSums;

        public Scene() : base(GameConst.ScreenSize)
        {
            Dock = DockStyle.Fill;

            _host = new Host();
            _tableSums = new TableSums();
            _buttonTakeMoney = new ButtonСapsule();
            _tableControls = new MovingTableControls((int)(GameConst.ScreenSize.Width * 0.3f), GameConst.ScreenSize.Height);
            _boxAnimation = new BoxAnimationTransition(GameConst.ScreenSize.Width - _tableControls.Width, (int)(GameConst.ScreenSize.Height * 0.36f));
            _boxQuestion = new BoxQuestion(_boxAnimation.Width, _boxAnimation.Height);
            _commandBoard = new CommandBoard(GameConst.ScreenSize.Width - _tableControls.Width, GameConst.ScreenSize.Height - _boxQuestion.Height);
            _tableHints = new TableHints(_tableControls.Width, (int)(_tableControls.Height * 0.2f));
            _prizeImage = new Bitmap(_boxAnimation.Width, _boxAnimation.Height);

            _boxAnimation.Location = _boxQuestion.Location = new Point(0, GameConst.ScreenSize.Height - _boxQuestion.Height);
            _buttonTakeMoney.Text = "Забрать деньги";

            using (var g = Graphics.FromImage(_prizeImage))
            using (var img = Resources.Question)
            {
                int height = _prizeImage.Width * img.Height / img.Width;
                int y = _prizeImage.Height - height >> 1;
                _boxAnimation.FontText = FontFactory.CreateFont(GameFonts.Arial, 0.6f * height);

                g.DrawImage(img, 0, y, _prizeImage.Width, height);
            }

            _commandBoard.CommandClick += OnCommandClick;
            _commandBoard.CancelClick += OnCancelClick;
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

        public event Action<bool> GameOver;
        public event Action<Achievements> AchievementCompleted;
        public event Action<StatisticsAttributes, int> StatisticsChanged;

        public Modes Mode { get; private set; } = Modes.Classic;

        public bool IsMenuAvailable { get; private set; } = false;

        private bool IsControlEnabled
        {
            set
            {
                _buttonTakeMoney.Enabled = value;
                _boxQuestion.Enabled = value;
                _tableHints.Enabled = value;
            }
        }

        private bool IsQuestionVisible
        {
            set
            {
                _boxQuestion.Visible = value;
                _boxAnimation.Visible = !value;
            }
        }

        public async void Start(bool isRestart = false)
        {
            IsMenuAvailable = true;
            Music.Play(Resources.Rules);

            await _tableControls.MoveX(GameConst.ScreenSize.Width - _tableControls.Width, 600 / GameConst.DeltaTime);

            _tableSums.Visible = _tableHints.Visible = true;

            if (isRestart)
            {
                _tableHints.ShowAllHints();
                _commandBoard.ButtonCommandVisible = Mode == Modes.Classic;

                OnCommandClick(this, Mode == Modes.Classic ? SceneCommands.About_Starting : SceneCommands.ChoosingSavingSum);
                return;
            }

            _commandBoard.Command = Mode == Modes.Classic ? SceneCommands.Show_SavingSums : SceneCommands.Show_HintsCount;
            _commandBoard.CancelCommand = SceneCancelCommands.SkipRules;
            _commandBoard.ButtonCommandEnabled = false;
            _commandBoard.Text = _host.Say(HostPhrases.Rules, $"{Question.MaxNumber}");

            await Task.Delay(1000);

            _commandBoard.ButtonsVisible = true;

            await _tableSums.ShowSums();

            _commandBoard.ButtonCommandEnabled = true;
        }

        public void Reset(Modes mode)
        {
            Mode = mode;
            IsQuestionVisible = IsMenuAvailable = false;

            foreach (Control ctrl in Controls)
                if (ctrl is IResettable res)
                    res.Reset(mode);
        }

        public void SetSettings(SettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is ISettable set)
                    set.SetSettings(data);
        }

        private async void OnOptionClick(LetterOptions letter)
        {
            switch (_boxQuestion.AnswerMode)
            {
                default:
                    IsControlEnabled = false;
                    StatisticsChanged?.Invoke(_boxQuestion.IsCorrectAnswer ? StatisticsAttributes.NumberCorrectAnswers : StatisticsAttributes.NumberIncorrectAnswers, 1);

                    ShowExplanationText();

                    if (_boxQuestion.IsCorrectAnswer && _boxQuestion.Question.Number < Question.MaxNumber)
                    {
                        _commandBoard.Command = SceneCommands.NextQuestion;
                    }
                    else
                    {
                        _buttonTakeMoney.Visible = false;
                        _commandBoard.Command = _boxQuestion.IsCorrectAnswer ? SceneCommands.Victory : SceneCommands.Loss;
                    }
                    break;

                case AnswerModes.DoubleDips:
                    if (_boxQuestion.IsCorrectAnswer == false)
                    {
                        await Task.Delay(3000);

                        if (_boxQuestion.Question.Difficulty > QuestionDifficulties.Easy)
                            Music.Stop();

                        _boxQuestion.AnswerMode = AnswerModes.Default;
                        _boxQuestion.LockOption(letter);

                        AchievementCompleted?.Invoke(Achievements.SuccessfulOutcome);

                        if (_boxQuestion.Question.OptionsCount == 2)
                        {
                            await Task.Delay(3000);

                            _boxQuestion.ClickCorrect();

                            AchievementCompleted?.Invoke(Achievements.NoOptions);
                        }
                        else
                        {
                            PlayLoopedHint(Resources.Hint_DoubleDip);
                            _boxQuestion.Enabled = true;
                            return;
                        }
                    }
                    else
                        goto default;
                    break;

                case AnswerModes.SwitchQuestion:
                    _commandBoard.Command = SceneCommands.SwitchQuestion;

                    var phrase1 = _boxQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestion_CorrectAnswer : HostPhrases.SwitchQuestion_IncorrectAnswer;

                    ShowExplanationText($"\n{_host.Say(phrase1, _boxQuestion.Question.Number.ToString())}");
                    break;

                case AnswerModes.TakeMoney:
                    _commandBoard.Command = SceneCommands.TakeMoney_ShowPrize;

                    var phrase2 = _boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    ShowExplanationText($"\n{_host.Say(phrase2, _tableSums.NextSum)}");
                    break;
            }

            _commandBoard.ButtonCommandVisible = true;
        }

        private async void OnHintClick(HintTypes type)
        {
            StatisticsChanged?.Invoke(StatisticsAttributes.NumberHintsUsed, 1);

            _buttonTakeMoney.Enabled = _tableHints.Enabled = type == HintTypes.FiftyFifty;

            switch (type)
            {
                case HintTypes.FiftyFifty:
                    _boxQuestion.Question.OptionsCount = 2;
                    _boxQuestion.SetQuestion(_boxQuestion.Question);

                    Sound.Play(Resources.Hint_FiftyFifty);
                    AchievementCompleted?.Invoke(Achievements.DearComputer);
                    break;

                case HintTypes.PhoneFriend:
                    _boxQuestion.Enabled = false;
                    _commandBoard.TextMode = CommandBoardTextModes.Dialog;
                    _commandBoard.Command = SceneCommands.End_PhoneFriend;

                    Music.Play(Resources.Hint_PhoneFriend_Dialing);

                    await _commandBoard.CallFriend(_boxQuestion.Question, _tableSums.NextSum);

                    Music.Stop();

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case HintTypes.AskAudience:
                    _boxQuestion.Enabled = false;
                    _commandBoard.Command = SceneCommands.End_AskAudience;

                    Music.Stop();

                    await _commandBoard.HoldVote(_boxQuestion.Question);

                    AchievementCompleted?.Invoke(Achievements.AudienceAward);

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case HintTypes.DoubleDip:
                    _boxQuestion.AnswerMode = AnswerModes.DoubleDips;
                    PlayLoopedHint(Resources.Hint_DoubleDip);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case HintTypes.SwitchQuestion:
                    _boxQuestion.AnswerMode = AnswerModes.SwitchQuestion;
                    _commandBoard.Text = _host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case HintTypes.AskHost:
                    _boxQuestion.Enabled = false;
                    PlayLoopedHint(Resources.Hint_AskHost, true);

                    _commandBoard.Text = new AskHostHint().GetHostAnswer(_boxQuestion.Question);

                    await _boxQuestion.ShowCentralIcon(type, true);

                    _commandBoard.Command = SceneCommands.End_AskHost;
                    _commandBoard.ButtonCommandVisible = true;
                    break;
            }
        }

        private async Task ShowQuestion(int number)
        {
            IsQuestionVisible = false;
            _boxQuestion.SetQuestion(number);

            await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

            IsQuestionVisible = true;

            await _boxQuestion.ShowQuestion();

            IsControlEnabled = true;
        }

        private async Task ShowCorrectAndPrize(bool playSound, bool addDelay, bool updatePrize, bool stopLooped)
        {
            _commandBoard.Clear();

            if (stopLooped)
                Music.Stop();

            await _boxQuestion.ShowCorrect(playSound, addDelay, _tableSums.IsCurrentSavingSum);

            if (_boxQuestion.Question.Number == 5 && _boxQuestion.IsCorrectAnswer && _tableSums.IsCurrentSavingSum == false && _boxQuestion.AnswerMode < AnswerModes.SwitchQuestion)
            {
                Sound.Play(Resources.Answer_Correct_Easy_Ending);
                Music.Stop();
            }

            if (updatePrize)
                _tableSums.UpdateNumberQuestion(_boxQuestion.IsCorrectAnswer);

            await _boxQuestion.Clear();
            await Task.Delay(500);

            IsQuestionVisible = false;

            await _boxAnimation.ShowTransition(_boxQuestion.BackgroundImage, _prizeImage);
            await _boxAnimation.ShowText(_tableSums.TextPrize);
        }

        private async void OnCommandClick(object sender, SceneCommands command)
        {
            switch (command)
            {
                case SceneCommands.NextQuestion:
                    var delay = 0;
                    var numberNextQuestion = _boxQuestion.Question.Number + 1;
                    var difficultyNexQuestion = Question.GetDifficulty(numberNextQuestion);

                    if (_tableSums.IsCurrentSavingSum == false && _boxQuestion.Question.Number == 5)
                        delay = 4500;
                    else
                        delay = _tableSums.IsCurrentSavingSum ? 7000 : 1500 + 500 * (int)_boxQuestion.Question.Difficulty;

                    if (_tableSums.IsCurrentSavingSum)
                        AchievementCompleted?.Invoke(Achievements.MoneyNotBurn);

                    await ShowCorrectAndPrize(true, false, true, _boxQuestion.Question.Difficulty > QuestionDifficulties.Easy || _tableSums.IsCurrentSavingSum);
                    await Task.Delay(delay);
                    await _boxAnimation.HideImage();

                    if (difficultyNexQuestion != QuestionDifficulties.Final)
                    {
                        await Task.Delay(1000);

                        if (difficultyNexQuestion != QuestionDifficulties.Easy)
                        {
                            Sound.Play(Resources.Question_Next);
                            await Task.Delay(3000);
                        }

                        if (Music.IsPlaying == false)
                            PlayMusicReflections(difficultyNexQuestion);

                        await ShowQuestion(numberNextQuestion);
                    }
                    else
                    {
                        _commandBoard.Text = _host.Say(HostPhrases.AboutFinalQuestion, Question.MaxNumber.ToString(), _tableSums.NextSum);
                        _commandBoard.Command = SceneCommands.FinalQuestion;
                        _commandBoard.ButtonCommandVisible = true;
                    }
                    break;

                case SceneCommands.FinalQuestion:
                    _commandBoard.Clear();
                    Sound.Play(Resources.Start);

                    await Task.Delay(5000);

                    PlayMusicReflections(QuestionDifficulties.Final);

                    await ShowQuestion(Question.MaxNumber);
                    break;

                case SceneCommands.Loss:
                case SceneCommands.Victory:
                    await ShowCorrectAndPrize(true, true, true, true);

                    StatisticsChanged?.Invoke(StatisticsAttributes.TotalPrize, _tableSums.Prize);

                    if (command == SceneCommands.Victory)
                    {
                        AchievementCompleted?.Invoke(Achievements.Millionaire);

                        if (_tableHints.UsedHintsCount == 0)
                            AchievementCompleted?.Invoke(Achievements.TriumphReason);

                        await Task.Delay(16000);
                    }

                    _commandBoard.AskRestart();
                    break;

                case SceneCommands.Show_SavingSums:
                    _commandBoard.ButtonCommandEnabled = false;
                    _commandBoard.Command = SceneCommands.Show_HintsCount;
                    _commandBoard.Text = _host.Say(HostPhrases.SavingSums, string.Join(", ", _tableSums.SavingSums.Select(sum => string.Format("{0:#,0}", sum))));

                    await _tableSums.ShowSavingSums();

                    _commandBoard.ButtonCommandEnabled = true;
                    break;

                case SceneCommands.Show_HintsCount:
                    _commandBoard.Command = SceneCommands.Show_Hint;
                    _commandBoard.Text = _host.Say(HostPhrases.HintsCount, _tableHints.TextActiveHints);
                    break;

                case SceneCommands.Show_Hint:
                    _commandBoard.Text = _tableHints.DescriptionNextHint;
                    _tableHints.ShowHint();

                    if (_tableHints.AreAllHintsShown)
                        _commandBoard.Command = _tableHints.HintsCount > Hint.MaxAllowedHintsCount ? SceneCommands.About_RestrictionsHints : SceneCommands.About_TakingMoney;
                    break;

                case SceneCommands.About_RestrictionsHints:
                    _commandBoard.Command = SceneCommands.About_TakingMoney;
                    _commandBoard.Text = _host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxAllowedHintsCount.ToString());
                    break;

                case SceneCommands.About_TakingMoney:
                    _commandBoard.Command = Mode == Modes.Classic ? SceneCommands.About_Starting : SceneCommands.ChoosingSavingSum;
                    _commandBoard.Text = _host.Say(HostPhrases.AboutTakingMoney);
                    break;

                case SceneCommands.ChoosingSavingSum:
                    _commandBoard.ButtonsVisible = false;
                    _commandBoard.Text = _host.Say(HostPhrases.AskSavingSum);

                    _tableSums.SavingSumSelected += OnSavingSumSelected;
                    _tableSums.EnableSelectionSavingSum();
                    break;

                case SceneCommands.About_Starting:
                    _commandBoard.ButtonCancelVisible = false;
                    _commandBoard.Command = SceneCommands.Start;
                    _commandBoard.Text = _host.Say(HostPhrases.GameStart);
                    break;

                case SceneCommands.Start:
                    _commandBoard.Clear();
                    _tableSums.Clear();

                    Music.Stop();
                    Sound.Play(Resources.Start);

                    await Task.Delay(3000);

                    PlayMusicReflections(QuestionDifficulties.Easy);

                    await ShowQuestion(Question.MinNumber);

                    _buttonTakeMoney.Visible = true;
                    break;

                case SceneCommands.End_PhoneFriend:
                    _commandBoard.Clear();
                    _commandBoard.TextMode = CommandBoardTextModes.Monologue;

                    if (sender is CommandBoard)
                    {
                        Sound.StopAll();
                        _commandBoard.StopTimer();
                    }

                    AchievementCompleted?.Invoke(Achievements.AndToTalk);

                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

                    await Task.Delay(2000);
                    await _commandBoard.RemoveMovingControls(500);

                    IsControlEnabled = true;
                    break;

                case SceneCommands.End_AskAudience:
                    _commandBoard.Clear();
                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

                    await _commandBoard.RemoveMovingControls(500);

                    IsControlEnabled = true;
                    break;

                case SceneCommands.SwitchQuestion:
                    _commandBoard.Clear();

                    int replacedVersionQuestion;

                    do
                        replacedVersionQuestion = Question.GetRandomVersion(_boxQuestion.Question.Number);
                    while (replacedVersionQuestion == _boxQuestion.Question.Version);

                    await _boxQuestion.ShowCorrect(false, true);
                    await _boxQuestion.Clear();

                    AchievementCompleted?.Invoke(Achievements.DefectiveQuestion);
                    IsQuestionVisible = false;

                    await _boxAnimation.HideImage(_boxQuestion.BackgroundImage);
                    await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

                    if (_boxQuestion.Question.OptionsCount == 2)
                        AchievementCompleted?.Invoke(Achievements.WasTwoBecameFour);

                    _boxQuestion.SetQuestion(_boxQuestion.Question.Number, replacedVersionQuestion);
                    IsQuestionVisible = true;

                    await _boxQuestion.ShowCentralIcon(HintTypes.SwitchQuestion, false);
                    await _boxQuestion.ShowQuestion();

                    IsControlEnabled = true;
                    break;

                case SceneCommands.End_AskHost:
                    _commandBoard.Clear();

                    AchievementCompleted?.Invoke(Achievements.NoOneWillKnow);

                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

                    await _boxQuestion.HideCentralIcon(true);

                    IsControlEnabled = true;
                    break;

                case SceneCommands.TakeMoney_Confirmation:
                    _commandBoard.Command = SceneCommands.TakeMoney;
                    _buttonTakeMoney.Visible = false;

                    var phrase = _tableSums.Prize > 0 ? HostPhrases.PlayerTakingMoney : HostPhrases.PlayerTakingMoney_Zero;

                    _commandBoard.Clear();
                    _commandBoard.Text = _host.Say(phrase, _tableSums.TextPrize);
                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case SceneCommands.TakeMoney:
                    _commandBoard.Clear();
                    StatisticsChanged?.Invoke(StatisticsAttributes.TotalPrize, _tableSums.Prize);

                    Music.Stop();
                    Sound.Play(Resources.PlayerTakesMoney);

                    await Task.Delay(7000);

                    _commandBoard.Text = _host.Say(HostPhrases.TakingMoney_AskAnswer);
                    _boxQuestion.AnswerMode = AnswerModes.TakeMoney;
                    _boxQuestion.Enabled = true;
                    break;

                case SceneCommands.TakeMoney_ShowPrize:
                    await ShowCorrectAndPrize(false, true, false, true);

                    AchievementCompleted?.Invoke(Achievements.StopGame);

                    if (_tableSums.Prize == 0)
                        AchievementCompleted?.Invoke(Achievements.IsPossible);

                    if (_tableSums.IsSavingSum(_boxQuestion.Question.Number - 1))
                        AchievementCompleted?.Invoke(Achievements.ExcessiveСaution);

                    _commandBoard.AskRestart();
                    break;

                case SceneCommands.Restart:
                    GameOver?.Invoke(true);
                    break;
            }
        }

        private void ShowExplanationText(string phraseOfHost = "")
        {
            var text = _boxQuestion.Question.Explanation;

            if (_boxQuestion.IsCorrectAnswer == false)
                text += $"\nПравильный ответ: {_boxQuestion.Question.FullCorrect}.";

            if (phraseOfHost != "")
                text += phraseOfHost;

            _commandBoard.Text = text;
        }

        private void PlayLoopedHint(UnmanagedMemoryStream stream, bool isIgnoreDifficulty = false)
        {
            if (isIgnoreDifficulty || _boxQuestion.Question.Difficulty != QuestionDifficulties.Easy)
                Music.Play(stream);
        }

        private void PlayMusicReflections(QuestionDifficulties difficulty)
        {
            if (difficulty == QuestionDifficulties.Easy)
                Music.Play(Resources.Question_Reflections_Easy);
            else
                Music.Play(Resources.Question_Reflections_Main);
        }

        private void OnSavingSumSelected(int sum)
        {
            _commandBoard.Text =
                $"{_host.Say(HostPhrases.SavingSumSelected, String.Format("{0:#,0}", sum))}\n" +
                $"{_host.Say(HostPhrases.GameStart)}";

            _tableSums.SavingSumSelected -= OnSavingSumSelected;

            _commandBoard.Command = SceneCommands.Start;
            _commandBoard.ButtonCommandVisible = true;
        }

        private void OnTakeMoneyClick(object sender, EventArgs e)
        {
            IsControlEnabled = false;
            _commandBoard.AskTakingMoney(_host.Say(HostPhrases.TakingMoney_ClarifyDecision));
        }

        private void OnCancelClick(object sender, SceneCancelCommands command)
        {
            switch (command)
            {
                case SceneCancelCommands.SkipRules:
                    _tableSums.CancelTask();
                    _tableHints.ShowAllHints();

                    OnCommandClick(this, Mode == Modes.Classic ? SceneCommands.About_Starting : SceneCommands.ChoosingSavingSum);
                    break;

                case SceneCancelCommands.CancelTakingMoney:
                    _commandBoard.Clear();
                    IsControlEnabled = true;
                    break;

                case SceneCancelCommands.ExitToMainMenu:
                    GameOver?.Invoke(IsMenuAvailable = false);
                    break;
            }
        }
    }
}