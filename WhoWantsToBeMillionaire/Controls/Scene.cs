using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Scene : GameContol, IReset, ISetSettings
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

        public Action<bool> GameOver;
        public Action<Achievement> AchievementCompleted;
        public Action<StatisticsAttribute, int> StatisticsChanged;

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
                _boxAnimation.FontText = FontManager.CreateFont(GameFont.Arial, 0.6f * height);

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

        public Mode Mode { get; private set; } = Mode.Classic;

        public bool MenuAllowed { get; private set; } = false;

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

        public void Reset(Mode mode)
        {
            Mode = mode;
            QuestionVisible = MenuAllowed = false;

            foreach (Control ctrl in Controls)
                if (ctrl is IReset res)
                    res.Reset(mode);
        }

        public void SetSettings(SettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is ISetSettings set)
                    set.SetSettings(data);
        }

        public async void Start(bool isRestart = false)
        {
            MenuAllowed = true;
            GameMusic.Play(Resources.Rules);

            await _tableControls.MoveX(GameConst.ScreenSize.Width - _tableControls.Width, 600 / GameConst.DeltaTime);

            _tableSums.Visible = _tableHints.Visible = true;

            if (isRestart)
            {
                _tableHints.ShowAllHints();
                _commandBoard.ButtonCommandVisible = Mode == Mode.Classic;

                OnCommandClick(this, Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
                return;
            }

            _commandBoard.Command = Mode == Mode.Classic ? SceneCommand.Show_SaveSums : SceneCommand.Show_CountHints;
            _commandBoard.CancelCommand = SceneCancelCommand.SkipRules;
            _commandBoard.ButtonCommandEnabled = false;
            _commandBoard.Text = _host.Say(HostPhrases.Rules, $"{Question.MaxNumber}");

            await Task.Delay(1000);

            _commandBoard.ButtonsVisible = true;

            await _tableSums.ShowSums();

            _commandBoard.ButtonCommandEnabled = true;
        }

        private async void OnOptionClick(LetterOption letter)
        {
            switch (_boxQuestion.AnswerMode)
            {
                default:
                    ControlEnabled = false;
                    StatisticsChanged?.Invoke(_boxQuestion.IsCorrectAnswer ? StatisticsAttribute.NumberCorrectAnswers : StatisticsAttribute.NumberIncorrectAnswers, 1);

                    ShowExplanationText();

                    if (_boxQuestion.IsCorrectAnswer && _boxQuestion.Question.Number < Question.MaxNumber)
                    {
                        _commandBoard.Command = SceneCommand.NextQuestion;
                    }
                    else
                    {
                        _buttonTakeMoney.Visible = false;
                        _commandBoard.Command = _boxQuestion.IsCorrectAnswer ? SceneCommand.Victory : SceneCommand.Loss;
                    }
                    break;

                case AnswerMode.DoubleDips:
                    if (!_boxQuestion.IsCorrectAnswer)
                    {
                        await Task.Delay(3000);

                        if (_boxQuestion.Question.Difficulty > DifficultyQuestion.Easy)
                            GameMusic.Stop();

                        _boxQuestion.AnswerMode = AnswerMode.Default;
                        _boxQuestion.LockOption(letter);

                        AchievementCompleted?.Invoke(Achievement.SuccessfulOutcome);

                        if (_boxQuestion.Question.CountOptions == 2)
                        {
                            await Task.Delay(3000);

                            _boxQuestion.ClickCorrect();

                            AchievementCompleted?.Invoke(Achievement.NoOptions);
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

                case AnswerMode.SwitchQuestion:
                    _commandBoard.Command = SceneCommand.SwitchQuestion;

                    var phrase1 = _boxQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestion_CorrectAnswer : HostPhrases.SwitchQuestion_IncorrectAnswer;

                    ShowExplanationText($"\n{_host.Say(phrase1, _boxQuestion.Question.Number.ToString())}");
                    break;

                case AnswerMode.TakeMoney:
                    _commandBoard.Command = SceneCommand.TakeMoney_ShowPrize;

                    var phrase2 = _boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    ShowExplanationText($"\n{_host.Say(phrase2, _tableSums.NextSum)}");
                    break;
            }

            _commandBoard.ButtonCommandVisible = true;
        }

        private void ShowExplanationText(string phraseOfHost = "")
        {
            var text = _boxQuestion.Question.Explanation;

            if (!_boxQuestion.IsCorrectAnswer)
                text += $"\nПравильный ответ: {_boxQuestion.Question.FullCorrect}.";

            if (phraseOfHost != "")
                text += phraseOfHost;

            _commandBoard.Text = text;
        }

        private async void OnHintClick(TypeHint type)
        {
            StatisticsChanged?.Invoke(StatisticsAttribute.NumberHintsUsed, 1);

            _buttonTakeMoney.Enabled = _tableHints.Enabled = type == TypeHint.FiftyFifty;

            switch (type)
            {
                case TypeHint.FiftyFifty:
                    var changedQuestion = new Question(_boxQuestion.Question, 2);
                    _boxQuestion.SetQuestion(changedQuestion);
                    GameSound.Play(Resources.Hint_FiftyFifty);

                    AchievementCompleted?.Invoke(Achievement.DearComputer);
                    break;

                case TypeHint.PhoneFriend:
                    _boxQuestion.Enabled = false;
                    _commandBoard.TextMode = CommandBoardTextMode.Dialog;
                    _commandBoard.Command = SceneCommand.End_PhoneFriend;

                    GameMusic.Play(Resources.Hint_PhoneFriend_Dialing);

                    await _commandBoard.CallFriend(_boxQuestion.Question, _tableSums.NextSum);

                    GameMusic.Stop();

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.AskAudience:
                    _boxQuestion.Enabled = false;
                    _commandBoard.Command = SceneCommand.End_AskAudience;

                    GameMusic.Stop();

                    await _commandBoard.HoldVote(_boxQuestion.Question);

                    AchievementCompleted?.Invoke(Achievement.AudienceAward);

                    _commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.DoubleDip:
                    _boxQuestion.AnswerMode = AnswerMode.DoubleDips;
                    PlayLoopedHint(Resources.Hint_DoubleDip);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.SwitchQuestion:
                    _boxQuestion.AnswerMode = AnswerMode.SwitchQuestion;
                    _commandBoard.Text = _host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await _boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.AskHost:
                    _boxQuestion.Enabled = false;
                    PlayLoopedHint(Resources.Hint_AskHost, true);

                    _commandBoard.Text = Hint.GetHostAnswer(_boxQuestion.Question);

                    await _boxQuestion.ShowCentralIcon(type, true);

                    _commandBoard.Command = SceneCommand.End_AskHost;
                    _commandBoard.ButtonCommandVisible = true;
                    break;
            }
        }

        private void PlayLoopedHint(UnmanagedMemoryStream stream, bool isIgnoreDifficulty = false)
        {
            if (isIgnoreDifficulty || _boxQuestion.Question.Difficulty != DifficultyQuestion.Easy)
                GameMusic.Play(stream);
        }

        private void PlayMusicReflections(DifficultyQuestion difficulty)
        {
            if (difficulty == DifficultyQuestion.Easy)
                GameMusic.Play(Resources.Question_Reflections_Easy);
            else
                GameMusic.Play(Resources.Question_Reflections_Main);
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

            await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

            QuestionVisible = true;

            await _boxQuestion.ShowQuestion();

            ControlEnabled = true;
        }

        private async Task ShowCorrectAndPrize(bool playSound, bool addDelay, bool updatePrize, bool stopLooped)
        {
            _commandBoard.Clear();

            if (stopLooped)
                GameMusic.Stop();

            await _boxQuestion.ShowCorrect(playSound, addDelay, _tableSums.NowSaveSum);

            if (_boxQuestion.Question.Number == 5 && _boxQuestion.IsCorrectAnswer && !_tableSums.NowSaveSum && _boxQuestion.AnswerMode < AnswerMode.SwitchQuestion)
            {
                GameSound.Play(Resources.Answer_Correct_Easy_Ending);
                GameMusic.Stop();
            }

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
                    var delay = 0;
                    var numberNextQuestion = _boxQuestion.Question.Number + 1;
                    var difficultyNexQuestion = Question.GetDifficulty(numberNextQuestion);

                    if (!_tableSums.NowSaveSum && _boxQuestion.Question.Number == 5)
                        delay = 4500;
                    else
                        delay = _tableSums.NowSaveSum ? 7000 : 1500 + 500 * (int)_boxQuestion.Question.Difficulty;

                    if (_tableSums.NowSaveSum)
                        AchievementCompleted?.Invoke(Achievement.MoneyNotBurn);

                    await ShowCorrectAndPrize(true, false, true, _boxQuestion.Question.Difficulty > DifficultyQuestion.Easy || _tableSums.NowSaveSum);
                    await Task.Delay(delay);
                    await _boxAnimation.HideImage();

                    if (difficultyNexQuestion != DifficultyQuestion.Final)
                    {
                        await Task.Delay(1000);

                        if (difficultyNexQuestion != DifficultyQuestion.Easy)
                        {
                            GameSound.Play(Resources.Question_Next);
                            await Task.Delay(3000);
                        }

                        if (!GameMusic.IsPlaying)
                            PlayMusicReflections(difficultyNexQuestion);

                        await ShowQuestion(numberNextQuestion);
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
                    GameSound.Play(Resources.Start);

                    await Task.Delay(5000);

                    PlayMusicReflections(DifficultyQuestion.Final);

                    await ShowQuestion(Question.MaxNumber);
                    break;

                case SceneCommand.Loss:
                case SceneCommand.Victory:
                    await ShowCorrectAndPrize(true, true, true, true);

                    StatisticsChanged?.Invoke(StatisticsAttribute.TotalPrize, _tableSums.Prize);

                    if (command == SceneCommand.Victory)
                    {
                        AchievementCompleted?.Invoke(Achievement.Millionaire);

                        if (_tableHints.CountUsedHints == 0)
                            AchievementCompleted?.Invoke(Achievement.TriumphReason);

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

                    if (_tableHints.AllHintsShown)
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

                    GameMusic.Stop();
                    GameSound.Play(Resources.Start);

                    await Task.Delay(3000);

                    PlayMusicReflections(DifficultyQuestion.Easy);

                    await ShowQuestion(1);

                    _buttonTakeMoney.Visible = true;
                    break;

                case SceneCommand.End_PhoneFriend:
                    _commandBoard.Clear();
                    _commandBoard.TextMode = CommandBoardTextMode.Monologue;

                    if (sender is CommandBoard)
                    {
                        GameSound.StopAll();
                        _commandBoard.StopTimer();
                    }

                    AchievementCompleted?.Invoke(Achievement.AndToTalk);

                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

                    await Task.Delay(2000);
                    await _commandBoard.RemoveMovingControls(500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskAudience:
                    _commandBoard.Clear();
                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

                    await _commandBoard.RemoveMovingControls(500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.SwitchQuestion:
                    _commandBoard.Clear();

                    int replacedVersionQuestion;

                    do
                        replacedVersionQuestion = Question.RandomVersion(_boxQuestion.Question.Number);
                    while (replacedVersionQuestion == _boxQuestion.Question.Version);

                    await _boxQuestion.ShowCorrect(false, true);
                    await _boxQuestion.Clear();

                    AchievementCompleted?.Invoke(Achievement.DefectiveQuestion);
                    QuestionVisible = false;

                    await _boxAnimation.HideImage(_boxQuestion.BackgroundImage);
                    await _boxAnimation.ShowImage(_boxQuestion.BackgroundImage);

                    if (_boxQuestion.Question.CountOptions == 2)
                        AchievementCompleted?.Invoke(Achievement.WasTwoBecameFour);

                    _boxQuestion.SetQuestion(_boxQuestion.Question.Number, replacedVersionQuestion);
                    QuestionVisible = true;

                    await _boxQuestion.ShowCentralIcon(TypeHint.SwitchQuestion, false);
                    await _boxQuestion.ShowQuestion();

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskHost:
                    _commandBoard.Clear();

                    AchievementCompleted?.Invoke(Achievement.NoOneWillKnow);

                    PlayMusicReflections(_boxQuestion.Question.Difficulty);

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
                    StatisticsChanged?.Invoke(StatisticsAttribute.TotalPrize, _tableSums.Prize);

                    GameMusic.Stop();
                    GameSound.Play(Resources.PlayerTakesMoney);

                    await Task.Delay(7000);

                    _commandBoard.Text = _host.Say(HostPhrases.TakingMoney_AskAnswer);
                    _boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    _boxQuestion.Enabled = true;
                    break;

                case SceneCommand.TakeMoney_ShowPrize:
                    await ShowCorrectAndPrize(false, true, false, true);

                    AchievementCompleted?.Invoke(Achievement.StopGame);

                    if (_tableSums.Prize == 0)
                        AchievementCompleted?.Invoke(Achievement.IsPossible);

                    if (_tableSums.CheckSaveSum(_boxQuestion.Question.Number - 1))
                        AchievementCompleted?.Invoke(Achievement.ExcessiveСaution);

                    _commandBoard.AskRestart();
                    break;

                case SceneCommand.Restart:
                    GameOver?.Invoke(true);
                    break;
            }
        }

        private void OnCancelClick(object sender, SceneCancelCommand command)
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
                    GameOver?.Invoke(MenuAllowed = false);
                    break;
            }
        }
    }
}