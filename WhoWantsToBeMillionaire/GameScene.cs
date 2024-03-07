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
        TakeMoney_ShowPrize
    }

    enum SceneCancelCommand
    {
        SkipRules,
        Cancel_TakingMoney,
        ExitToMainMenu
    }

    class GameScene : GameContol
    {
        private readonly Bitmap prizeImage;
        private readonly ButtonEllipse buttonTakeMoney;
        private readonly BoxAnimation boxAnimation;
        private readonly BoxQuestion boxQuestion;
        private readonly Host host;
        private readonly Hint hint;
        private readonly CommandBoard commandBoard;
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;

        private PhoneTimer timer;
        private VotingChart chart;
        private Mode mode;

        public delegate void EventSceneRestarting();
        public delegate void EventExitToMainMenu();
        public delegate void EventStatisticsChanged(StatsAttribute key, int value = 1);

        public event EventStatisticsChanged StatisticsChanged;
        public event EventSceneRestarting SceneRestarting;
        public event EventExitToMainMenu ExitToMainMenu;

        private bool ControlEnabled
        {
            set
            {
                buttonTakeMoney.Enabled = value;
                boxQuestion.Enabled = value;
                tableHints.Enabled = value;
            }
        }

        public GameScene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            host = new Host();
            tableSums = new TableSums((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height);
            boxAnimation = new BoxAnimation(MainForm.RectScreen.Width - tableSums.Width, (int)(MainForm.RectScreen.Height * 0.36f));
            boxQuestion = new BoxQuestion(boxAnimation.Width, boxAnimation.Height);
            buttonTakeMoney = new ButtonEllipse((int)(0.8f * tableSums.Width), (int)(0.05f * tableSums.Height));
            commandBoard = new CommandBoard(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - boxQuestion.Height);
            tableHints = new TableHints(tableSums.Width, (int)(tableSums.Height * 0.2f));
            hint = new Hint();

            boxAnimation.Location = boxQuestion.Location = new Point(0, MainForm.RectScreen.Height - boxQuestion.Height);
            buttonTakeMoney.Location = new Point((tableSums.Width - buttonTakeMoney.Width) / 2, tableSums.Height - 2 * buttonTakeMoney.Height);

            buttonTakeMoney.Text = "Забрать деньги";

            prizeImage = new Bitmap(boxAnimation.Width, boxAnimation.Height);

            using (Graphics g = Graphics.FromImage(prizeImage))
            using (Image img = ResourceProcessing.GetImage("Question.png"))
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
        }

        public void Reset()
        {
            mode = (Mode)Properties.Settings.Default.Mode;

            tableSums.Reset(mode);
            tableHints.Reset(mode);
            commandBoard.Reset();
            boxQuestion.Reset();
            boxAnimation.Reset();

            buttonTakeMoney.Visible = false;

            boxQuestion.Visible = false;
        }

        public async void Start()
        {
            commandBoard.Command = mode == Mode.Classic ? SceneCommand.Show_SaveSums : SceneCommand.Show_CountHints;
            commandBoard.CancelCommand = SceneCancelCommand.SkipRules;

            await tableSums.Show();

            tableHints.Visible = true;

            commandBoard.ButtonCommandEnabled = false;
            commandBoard.ButtonsVisible = true;

            commandBoard.Text = host.Say(HostPhrases.Rules, tableSums.MaxNumberSum.ToString());

            await Task.Delay(1000);
            await tableSums.ShowSums();

            commandBoard.ButtonCommandEnabled = true;
        }

        public async void Restart()
        {
            await tableSums.Show();

            tableHints.Visible = true;
            tableHints.ShowAllHints();

            commandBoard.ButtonCommandVisible = true;

            OnCommandClick(this, mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
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

                    if (boxQuestion.IsCorrectAnswer && boxQuestion.Question.Number < tableSums.MaxNumberSum)
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

                        boxQuestion.LockOption(letter);

                        if (boxQuestion.Question.CountOptions == 2)
                        {
                            await Task.Delay(3000);
                            boxQuestion.ClickCorrect();
                        }
                        else
                        {
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
                    boxQuestion.SetText(hint.ReduceOptions(boxQuestion.Question));
                    break;

                case TypeHint.PhoneFriend:
                    boxQuestion.Enabled = false;
                    commandBoard.Command = SceneCommand.End_PhoneFriend;

                    commandBoard.Reset();
                    commandBoard.ContentAlignment = ContentAlignment.MiddleLeft;

                    timer = new PhoneTimer((int)(0.3f * commandBoard.Height));
                    timer.TimeUp += OnCommandClick;

                    await Task.Delay(3000);

                    foreach (var phrase in hint.PhoneFriendDialog(tableSums.NextSum))
                    {
                        commandBoard.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await commandBoard.ShowMovingPictureBox(timer, false, 500 / MainForm.DeltaTime);

                    commandBoard.Text = hint.PhoneFriendAnswer(boxQuestion.Question);
                    timer.Start();

                    commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.AskAudience:
                    boxQuestion.Enabled = false;
                    commandBoard.Command = SceneCommand.End_AskAudience;

                    int heigth = (int)(0.7f * commandBoard.Height);
                    chart = new VotingChart((int)(0.75f * heigth), heigth);

                    await commandBoard.ShowMovingPictureBox(chart, true, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, hint.PercentsAudience(boxQuestion.Question));

                    commandBoard.ButtonCommandVisible = true;
                    break;

                case TypeHint.DoubleDip:
                    boxQuestion.AnswerMode = AnswerMode.DoubleDips;

                    await boxQuestion.ShowCentralIcon(type);
                    break;

                case TypeHint.SwitchQuestion:
                    boxQuestion.AnswerMode = AnswerMode.SwitchQuestion;
                    commandBoard.Text = host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await boxQuestion.ShowCentralIcon(type);
                    break;

                case TypeHint.AskHost:
                    boxQuestion.Enabled = false;
                    commandBoard.Text = hint.HostAnswer(boxQuestion.Question);

                    await boxQuestion.ShowCentralIcon(type);

                    commandBoard.Command = SceneCommand.End_AskHost;
                    commandBoard.ButtonCommandVisible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            await commandBoard.RemoveMovingPictureBox(box, countFrames);
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

        private async Task ShowCorrectAndPrize(bool addDelay, bool updatePrize)
        {
            commandBoard.Clear();
            await boxQuestion.ShowCorrect(addDelay);

            if (updatePrize)
                tableSums.UpdatePrize(boxQuestion.IsCorrectAnswer);

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
                    await ShowCorrectAndPrize(false, true);
                    await Task.Delay(3000);
                    await boxAnimation.HideImage();

                    await Task.Delay(3000);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    boxQuestion.Visible = true;

                    await boxQuestion.ShowQuestion(boxQuestion.Question.Number + 1);

                    ControlEnabled = true;
                    break;

                case SceneCommand.Loss:
                    await ShowCorrectAndPrize(true, true);
                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
                    commandBoard.AskRestart();
                    break;

                case SceneCommand.Victory:
                    await ShowCorrectAndPrize(true, true);
                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
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
                    commandBoard.Text = host.Say(HostPhrases.CountHints, tableHints.StringCountActiveHints);
                    break;

                case SceneCommand.Show_Hint:
                    commandBoard.Text = hint.Description(tableHints.PeekHiddenHint);
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        commandBoard.Command = tableHints.CountHints > Hint.MaxCountAllowedHints ? SceneCommand.About_RestrictionsHints : SceneCommand.About_TakingMoney;
                    break;

                case SceneCommand.About_RestrictionsHints:
                    commandBoard.Command = SceneCommand.About_TakingMoney;
                    commandBoard.Text = host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxCountAllowedHints.ToString());
                    break;

                case SceneCommand.About_TakingMoney:
                    commandBoard.Command = mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum;
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
                    tableSums.NumberNextSum = 1;

                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    boxQuestion.Visible = true;

                    await boxQuestion.ShowQuestion(1);

                    ControlEnabled = true;
                    buttonTakeMoney.Visible = true;
                    break;

                case SceneCommand.End_PhoneFriend:
                case SceneCommand.End_AskAudience:
                    commandBoard.Clear();

                    if (commandBoard.Command == SceneCommand.End_PhoneFriend)
                    {
                        commandBoard.ContentAlignment = ContentAlignment.MiddleCenter;

                        timer.Stop();
                        timer.TimeUp -= OnCommandClick;

                        await Task.Delay(2000);
                        await RemoveMovingPictureBox(timer, 500 / MainForm.DeltaTime);
                    }
                    else
                    {
                        await RemoveMovingPictureBox(chart, 1000 / MainForm.DeltaTime);
                    }

                    ControlEnabled = true;
                    break;

                case SceneCommand.SwitchQuestion:
                    commandBoard.Clear();

                    int newIndex;
                    do
                        newIndex = Question.RandomIndex(boxQuestion.Question.Number);
                    while (newIndex == boxQuestion.Question.Index);

                    await boxQuestion.ShowCorrect(true);
                    await Task.Delay(3000);
                    await boxQuestion.Clear();

                    boxQuestion.Visible = false;

                    await boxAnimation.HideImage(boxQuestion.BackgroundImage);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    boxQuestion.Visible = true;

                    boxQuestion.Reset();

                    await boxQuestion.ShowCentralIcon(TypeHint.SwitchQuestion);
                    await boxQuestion.ShowQuestion(boxQuestion.Question.Number, newIndex);

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskHost:
                    commandBoard.Clear();

                    await boxQuestion.HideCentralIcon();

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

                    await Task.Delay(3000);

                    commandBoard.Text = host.Say(HostPhrases.TakingMoney_AskAnswer);
                    boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.TakeMoney_ShowPrize:
                    await ShowCorrectAndPrize(true, false);
                    commandBoard.AskRestart();
                    break;

                case SceneCommand.Restart:
                    SceneRestarting.Invoke();
                    break;
            }
        }

        private void OnCacnelClick(object sender, SceneCancelCommand command)
        {
            switch (command)
            {
                case SceneCancelCommand.SkipRules:
                    tableSums.CancelTask();
                    tableHints.ShowAllHints();

                    OnCommandClick(this, mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum);
                    break;

                case SceneCancelCommand.Cancel_TakingMoney:
                    commandBoard.Clear();
                    ControlEnabled = true;
                    break;

                case SceneCancelCommand.ExitToMainMenu:
                    ExitToMainMenu.Invoke();
                    break;
            }
        }
    }
}
