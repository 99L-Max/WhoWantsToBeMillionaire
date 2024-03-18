using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        FinalQuestion,
        Debug_FirstQuestion,
        Debug_Next
    }

    enum SceneCancelCommand
    {
        SkipRules,
        Cancel_TakingMoney,
        ExitToMainMenu
    }

    class Scene : GameContol, IReset, IGameSettings
    {
        private readonly Bitmap prizeImage;
        private readonly ButtonСapsule buttonTakeMoney;
        private readonly BoxAnimation boxAnimation;
        private readonly BoxQuestion boxQuestion;
        private readonly Host host;
        private readonly Hint hint;
        private readonly CommandBoard commandBoard;
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;
        private readonly IReset[] resets;
        private readonly IGameSettings[] settings;

        private PhoneTimer timer;
        private VotingChart chart;

        public delegate void EventGameOver(bool isRestart);
        public delegate void EventStatisticsChanged(StatsAttribute key, int value = 1);

        public event EventStatisticsChanged StatisticsChanged;
        public event EventGameOver GameOver;

        public Mode Mode { private set; get; } = Mode.Classic;

        public bool MenuAllowed { private set; get; } = false;

        private bool ControlEnabled
        {
            set
            {
                buttonTakeMoney.Enabled = value;
                boxQuestion.Enabled = value;
                tableHints.Enabled = value;
            }
        }

        public Scene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            host = new Host();
            tableSums = new TableSums((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height);
            boxAnimation = new BoxAnimation(MainForm.RectScreen.Width - tableSums.Width, (int)(MainForm.RectScreen.Height * 0.36f));
            boxQuestion = new BoxQuestion(boxAnimation.Width, boxAnimation.Height);
            buttonTakeMoney = new ButtonСapsule((int)(0.8f * tableSums.Width), (int)(0.05f * tableSums.Height));
            commandBoard = new CommandBoard(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - boxQuestion.Height);
            tableHints = new TableHints(tableSums.Width, (int)(tableSums.Height * 0.2f));
            hint = new Hint();

            boxAnimation.Location = boxQuestion.Location = new Point(0, MainForm.RectScreen.Height - boxQuestion.Height);
            buttonTakeMoney.Location = new Point((tableSums.Width - buttonTakeMoney.Width) / 2, tableSums.Height - 2 * buttonTakeMoney.Height);

            buttonTakeMoney.Text = "Забрать деньги";

            prizeImage = new Bitmap(boxAnimation.Width, boxAnimation.Height);

            using (Graphics g = Graphics.FromImage(prizeImage))
            using (Image img = ResourceManager.GetImage("Question.png"))
            {
                int height = prizeImage.Width * img.Height / img.Width;
                int y = (prizeImage.Height - height) >> 1;
                boxAnimation.SizeFont = 0.42f * height;

                g.DrawImage(img, 0, y, prizeImage.Width, height);
            }

            boxQuestion.VisibleChanged += (s, e) => boxAnimation.Visible = !boxQuestion.Visible;

            commandBoard.CommandClick += OnCommandClick;
            commandBoard.CancelClick += OnCacnelClick;
            buttonTakeMoney.Click += OnButtonTakeMoneyClick;
            tableHints.HintClick += OnHintClick;
            boxQuestion.OptionClick += OnOptionClick;

            tableSums.Controls.Add(tableHints);
            tableSums.Controls.Add(buttonTakeMoney);

            Controls.Add(tableSums);
            Controls.Add(commandBoard);
            Controls.Add(boxAnimation);
            Controls.Add(boxQuestion);

            resets = new IReset[] { tableHints, tableSums, boxQuestion, boxAnimation, commandBoard };
            settings = new IGameSettings[] { tableHints, boxQuestion };
        }

        public void Reset(Mode mode)
        {
            Mode = mode;

            foreach (var ctrl in resets)
                ctrl.Reset(mode);

            buttonTakeMoney.Visible = boxQuestion.Visible = false;
            MenuAllowed = false;
        }

        public async void Start()
        {
            MenuAllowed = true;

            Sound.PlayBackground("Rules.wav");

            commandBoard.Command = Mode == Mode.Classic ? SceneCommand.Show_SaveSums : SceneCommand.Show_CountHints;
            commandBoard.CancelCommand = SceneCancelCommand.SkipRules;

            await tableSums.Show();

            tableHints.Visible = true;
            commandBoard.ButtonCommandEnabled = false;
            commandBoard.Text = host.Say(HostPhrases.Rules, Question.MaxNumber.ToString()); ;

            await Task.Delay(1000);

            commandBoard.ButtonsVisible = true;

            await tableSums.ShowSums();

            commandBoard.ButtonCommandEnabled = true;
        }

        public async void Restart()
        {
            MenuAllowed = true;

            Sound.PlayBackground("Rules.wav");

            await tableSums.Show();

            tableHints.Visible = true;
            tableHints.ShowAllHints();

            commandBoard.ButtonCommandVisible = true;

            OnCommandClick(this, Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
        }

        private async void OnOptionClick(Letter letter)
        {
            string explanation = boxQuestion.Question.Explanation;
            if (!boxQuestion.IsCorrectAnswer)
                explanation += $"\nПравильный ответ: {boxQuestion.Question.FullCorrect}.";

            switch (boxQuestion.AnswerMode)
            {
                default:
                    ControlEnabled = false;
                    StatisticsChanged.Invoke(boxQuestion.IsCorrectAnswer ? StatsAttribute.NumberCorrectAnswers : StatsAttribute.NumberIncorrectAnswers);

                    if (boxQuestion.IsCorrectAnswer && boxQuestion.Question.Number < Question.MaxNumber)
                    {
                        commandBoard.Command = SceneCommand.NextQuestion;
                    }
                    else
                    {
                        buttonTakeMoney.Visible = false;
                        commandBoard.Command = boxQuestion.IsCorrectAnswer ? SceneCommand.Victory : SceneCommand.Loss;
                    }

                    commandBoard.Text = explanation;
                    break;

                case AnswerMode.DoubleDips:
                    if (!boxQuestion.IsCorrectAnswer)
                    {
                        boxQuestion.AnswerMode = AnswerMode.Usual;
                        await Task.Delay(3000);

                        Sound.StopAll();
                        boxQuestion.LockOption(letter);

                        if (boxQuestion.Question.CountOptions == 2)
                        {
                            await Task.Delay(3000);
                            boxQuestion.ClickCorrect();
                        }
                        else
                        {
                            boxQuestion.PlayBackgroundSound("Hint_DoubleDip.wav");
                            boxQuestion.Enabled = true;
                            return;
                        }
                    }
                    else
                        goto default;
                    break;

                case AnswerMode.SwitchQuestion:
                    commandBoard.Command = SceneCommand.SwitchQuestion;

                    HostPhrases phrase1 = boxQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestion_CorrectAnswer : HostPhrases.SwitchQuestion_IncorrectAnswer;

                    commandBoard.Text = $"{explanation}\n{host.Say(phrase1, boxQuestion.Question.Number.ToString())}";
                    break;

                case AnswerMode.TakeMoney:
                    commandBoard.Command = SceneCommand.TakeMoney_ShowPrize;

                    HostPhrases phrase2 = boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    commandBoard.Text = $"{explanation}\n{host.Say(phrase2, tableSums.NextSum.ToString())}";
                    break;
            }

            commandBoard.ButtonCommandVisible = true;
        }

        private async void OnHintClick(TypeHint type)
        {
            StatisticsChanged.Invoke(StatsAttribute.NumberHintsUsed);

            buttonTakeMoney.Enabled = tableHints.Enabled = type == TypeHint.FiftyFifty;

            switch (type)
            {
                case TypeHint.FiftyFifty:
                    Sound.Play("Hint_FiftyFifty.wav");
                    boxQuestion.SetQuestion(hint.ReduceOptions(boxQuestion.Question));
                    break;

                case TypeHint.PhoneFriend:
                    Sound.PlayBackground("Hint_PhoneFriend_Dialing.wav");

                    boxQuestion.Enabled = false;
                    commandBoard.Command = SceneCommand.End_PhoneFriend;

                    commandBoard.Reset();
                    commandBoard.ContentAlignment = ContentAlignment.MiddleLeft;

                    timer = new PhoneTimer((int)(0.3f * commandBoard.Height));
                    timer.TimeUp += OnCommandClick;

                    await Task.Delay(2000);

                    Sound.Play("Hint_PhoneFriend_Beeps.wav");

                    await Task.Delay(4000);

                    foreach (var phrase in hint.PhoneFriendDialog(tableSums.NextSum))
                    {
                        commandBoard.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await commandBoard.ShowMovingPictureBox(timer, 500, false);

                    commandBoard.Text = hint.PhoneFriendAnswer(boxQuestion.Question);
                    timer.Start();

                    commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.AskAudience:
                    Sound.PlayBackground("Hint_AskAudience_Begin.wav");

                    boxQuestion.Enabled = false;
                    commandBoard.Command = SceneCommand.End_AskAudience;

                    int heigth = (int)(0.7f * commandBoard.Height);
                    chart = new VotingChart((int)(0.75f * heigth), heigth);

                    await commandBoard.ShowMovingPictureBox(chart, 500, true);
                    await Task.Delay(3000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, hint.PercentsAudience(boxQuestion.Question));

                    commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.DoubleDip:
                    boxQuestion.AnswerMode = AnswerMode.DoubleDips;
                    boxQuestion.PlayBackgroundSound("Hint_DoubleDip.wav");

                    await boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.SwitchQuestion:
                    boxQuestion.AnswerMode = AnswerMode.SwitchQuestion;
                    commandBoard.Text = host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await boxQuestion.ShowCentralIcon(type, true);
                    break;

                case TypeHint.AskHost:
                    boxQuestion.Enabled = false;
                    boxQuestion.PlayBackgroundSound("Hint_AskHost.wav");

                    commandBoard.Text = hint.HostAnswer(boxQuestion.Question);

                    await boxQuestion.ShowCentralIcon(type, true);

                    commandBoard.Command = SceneCommand.End_AskHost;
                    commandBoard.ButtonCommandVisible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int milliseconds)
        {
            await commandBoard.RemoveMovingPictureBox(box, milliseconds / MainForm.DeltaTime);
            box.Dispose();
        }

        private void SaveSumSelected(int sum)
        {
            commandBoard.Text = host.Say(HostPhrases.SaveSumSelected, String.Format("{0:#,0}", sum)) + "\n" + host.Say(HostPhrases.GameStart);
            tableSums.SaveSumSelected -= SaveSumSelected;

            commandBoard.Command = SceneCommand.Start;
            commandBoard.ButtonCommandVisible = true;
        }

        private void OnButtonTakeMoneyClick(object sender, EventArgs e)
        {
            ControlEnabled = false;
            commandBoard.AskTakingMoney(host.Say(HostPhrases.TakingMoney_ClarifyDecision));
        }

        private async Task ShowQuestion(int number)
        {
            boxQuestion.SetQuestion(number);

            if (boxQuestion.Question.Difficulty != DifficultyQuestion.Easy)
            {
                if (boxQuestion.Question.Difficulty == DifficultyQuestion.Final)
                {
                    Sound.Play("Start.wav");
                    await Task.Delay(5000);
                }
                else
                {
                    Sound.Play("Question_Next.wav");
                    await Task.Delay(3000);
                }

                Sound.PlayBackground("Question_Reflections.wav");
            }

            await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

            boxQuestion.Visible = true;

            await boxQuestion.ShowQuestion();

            ControlEnabled = true;
        }

        private async Task ShowCorrectAndPrize(bool playSound, bool addDelay, bool updatePrize)
        {
            commandBoard.Clear();
            Sound.StopAll();

            await boxQuestion.ShowCorrect(playSound, addDelay, tableSums.NowSaveSum);

            if (boxQuestion.Question.Number == 5 && !tableSums.NowSaveSum)
                Sound.Play("Answer_Correct_Easy_Ending.wav");

            if (updatePrize)
                tableSums.Update(boxQuestion.IsCorrectAnswer);

            await boxQuestion.Clear();
            await Task.Delay(500);

            boxQuestion.Visible = false;

            await boxAnimation.ShowTransition(boxQuestion.BackgroundImage, prizeImage);
            await boxAnimation.ShowText(tableSums.TextPrize);
        }

        private async void OnCommandClick(object sender, SceneCommand command)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    int delay;

                    if (!tableSums.NowSaveSum && boxQuestion.Question.Number == 5)
                        delay = 3500;
                    else
                        delay = tableSums.NowSaveSum ? 7000 : 1500 + 500 * (int)boxQuestion.Question.Difficulty;

                    await ShowCorrectAndPrize(true, false, true);
                    await Task.Delay(delay);
                    await boxAnimation.HideImage();

                    if (boxQuestion.Question.Number + 1 < Question.MaxNumber)
                    {
                        await Task.Delay(1000);
                        await ShowQuestion(boxQuestion.Question.Number + 1);
                    }
                    else
                    {
                        commandBoard.Text = host.Say(HostPhrases.AboutFinalQuestion, Question.MaxNumber.ToString(), tableSums.NextSum);
                        commandBoard.Command = SceneCommand.FinalQuestion;
                        commandBoard.ButtonCommandVisible = true;
                    }
                    break;

                case SceneCommand.FinalQuestion:
                    commandBoard.Clear();
                    await ShowQuestion(Question.MaxNumber);
                    break;

                case SceneCommand.Loss:
                    await ShowCorrectAndPrize(true, true, true);

                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
                    commandBoard.AskRestart();
                    break;

                case SceneCommand.Victory:
                    await ShowCorrectAndPrize(true, true, true);

                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);

                    await Task.Delay(16000);

                    commandBoard.AskRestart();
                    break;

                case SceneCommand.Show_SaveSums:
                    commandBoard.ButtonCommandEnabled = false;
                    commandBoard.Command = SceneCommand.Show_CountHints;

                    commandBoard.Text = host.Say(HostPhrases.SaveSums, string.Join(", ", Array.ConvertAll(tableSums.SaveSums, x => string.Format("{0:#,0}", x))));

                    await tableSums.ShowSaveSums();

                    commandBoard.ButtonCommandEnabled = true;
                    break;

                case SceneCommand.Show_CountHints:
                    commandBoard.Command = SceneCommand.Show_Hint;
                    commandBoard.Text = host.Say(HostPhrases.CountHints, tableHints.TextActiveHints);
                    break;

                case SceneCommand.Show_Hint:
                    commandBoard.Text = tableHints.DescriptionNextHint;
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        commandBoard.Command = tableHints.CountHints > Hint.MaxCountAllowedHints ? SceneCommand.About_RestrictionsHints : SceneCommand.About_TakingMoney;
                    break;

                case SceneCommand.About_RestrictionsHints:
                    commandBoard.Command = SceneCommand.About_TakingMoney;
                    commandBoard.Text = host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxCountAllowedHints.ToString());
                    break;

                case SceneCommand.About_TakingMoney:
                    commandBoard.Command = Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum;
                    commandBoard.Text = host.Say(HostPhrases.AboutTakingMoney);
                    break;

                case SceneCommand.ChoosingSaveSum:
                    commandBoard.ButtonsVisible = false;
                    commandBoard.Text = host.Say(HostPhrases.AskSaveSum);

                    tableSums.SaveSumSelected += SaveSumSelected;
                    tableSums.AddSelectionSaveSum();
                    break;

                case SceneCommand.About_Starting:
                    commandBoard.ButtonCancelVisible = false;
                    commandBoard.Command = SceneCommand.Start;
                    commandBoard.Text = host.Say(HostPhrases.GameStart);
                    break;

                case SceneCommand.Start:
                    commandBoard.Clear();
                    tableSums.Clear();

                    Sound.StopBackground();
                    Sound.Play("Start.wav");

                    await Task.Delay(3000);
                    await ShowQuestion(1);

                    buttonTakeMoney.Visible = true;
                    break;

                case SceneCommand.End_PhoneFriend:
                    commandBoard.Clear();
                    commandBoard.ContentAlignment = ContentAlignment.MiddleCenter;

                    timer.Stop();
                    timer.TimeUp -= OnCommandClick;

                    if (sender is CommandBoard)
                    {
                        Sound.StopPeek();
                        Sound.Play("Hint_PhoneFriend_End.wav");
                    }

                    boxQuestion.PlayBackgroundSound("Question_Reflections.wav");

                    await Task.Delay(2000);
                    await RemoveMovingPictureBox(timer, 500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskAudience:
                    commandBoard.Clear();

                    boxQuestion.PlayBackgroundSound("Question_Reflections.wav");

                    await RemoveMovingPictureBox(chart, 500);

                    ControlEnabled = true;
                    break;

                case SceneCommand.SwitchQuestion:
                    commandBoard.Clear();

                    int newIndex;
                    do
                        newIndex = Question.RandomIndex(boxQuestion.Question.Number);
                    while (newIndex == boxQuestion.Question.Index);

                    await boxQuestion.ShowCorrect(false, true);
                    await Task.Delay(3000);
                    await boxQuestion.Clear();

                    boxQuestion.Visible = false;

                    await boxAnimation.HideImage(boxQuestion.BackgroundImage);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    boxQuestion.Visible = true;
                    boxQuestion.SetQuestion(boxQuestion.Question.Number, newIndex);

                    await boxQuestion.ShowCentralIcon(TypeHint.SwitchQuestion, false);
                    await boxQuestion.ShowQuestion();

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskHost:
                    commandBoard.Clear();

                    boxQuestion.PlayBackgroundSound("Question_Reflections.wav");

                    await boxQuestion.HideCentralIcon(true);

                    ControlEnabled = true;
                    break;

                case SceneCommand.TakeMoney_Confirmation:
                    commandBoard.Command = SceneCommand.TakeMoney;

                    buttonTakeMoney.Visible = false;

                    commandBoard.Clear();
                    commandBoard.Text = host.Say(HostPhrases.PlayerTakingMoney, tableSums.TextPrize);
                    commandBoard.ButtonCommandVisible = true;
                    break;

                case SceneCommand.TakeMoney:
                    commandBoard.Clear();

                    Sound.StopAll();
                    Sound.Play("PlayerTakesMoney.wav");
                    await Task.Delay(7000);

                    commandBoard.Text = host.Say(HostPhrases.TakingMoney_AskAnswer);
                    boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.TakeMoney_ShowPrize:
                    await ShowCorrectAndPrize(false, true, false);
                    commandBoard.AskRestart();
                    break;

                case SceneCommand.Restart:
                    Sound.StopAll();
                    GameOver.Invoke(true);
                    break;

                case SceneCommand.Debug_FirstQuestion:
                    Sound.StopAll();
                    tableSums.Clear();
                    await ShowQuestion(1);
                    Debug_SetQuestion(1, 1);
                    break;

                case SceneCommand.Debug_Next:
                    int number = boxQuestion.Question.Number;
                    int index = boxQuestion.Question.Index;

                    try { Debug_SetQuestion(number, ++index); }
                    catch (Exception)
                    {
                        try { Debug_SetQuestion(++number, 1); }
                        catch 
                        {
                            commandBoard.Clear();
                            commandBoard.Text = "Конец";
                            commandBoard.Command = SceneCommand.Restart;
                        }
                    }
                    break;
            }
        }

        private void Debug_SetQuestion(int number, int index)
        {
            ControlEnabled = false;

            Question question = new Question(number, index);

            boxQuestion.SetQuestion(question);
            commandBoard.Text = $"{question.Explanation}\nПравильный ответ: {question.FullCorrect}\n№{number:d2}.{index:d2}";

            commandBoard.Command = SceneCommand.Debug_Next;
            commandBoard.ButtonCommandVisible = true;
        }

        private void OnCacnelClick(object sender, SceneCancelCommand command)
        {
            switch (command)
            {
                case SceneCancelCommand.SkipRules:
                    tableSums.CancelTask();
                    tableHints.ShowAllHints();

                    OnCommandClick(this, Mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
                    //OnCommandClick(this, SceneCommand.Debug_FirstQuestion);
                    break;

                case SceneCancelCommand.Cancel_TakingMoney:
                    commandBoard.Clear();
                    ControlEnabled = true;
                    break;

                case SceneCancelCommand.ExitToMainMenu:
                    MenuAllowed = false;
                    GameOver.Invoke(false);
                    break;
            }
        }

        public void SetSettings(GameSettingsData data)
        {
            foreach (var ctrl in settings)
                ctrl.SetSettings(data);
        }
    }
}
