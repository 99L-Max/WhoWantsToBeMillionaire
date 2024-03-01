using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum SceneCommand
    {
        Start,
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
        Cancel_TakingMoney
    }

    class GameScene : GameContol
    {
        private readonly Bitmap prizeImage;
        private readonly ButtonEllipse buttonTakeMoney;
        private readonly BoxAnimation boxAnimation;
        private readonly BoxQuestion boxQuestion;
        private readonly Host host;
        private readonly Hint hint;
        private readonly Dialog dialog;
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;

        private PhoneTimer timer;
        private VotingChart chart;
        private SceneCommand command;
        private SceneCommand cancelCommand;
        private Mode mode;

        public delegate void EventStatisticsChanged(StatsAttribute key, int value = 1);
        public event EventStatisticsChanged StatisticsChanged;

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
            dialog = new Dialog(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - boxQuestion.Height);
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

            dialog.CommandClick += OnCommandClick;
            dialog.CancelClick += OnCacnelClick;
            buttonTakeMoney.Click += OnButtonTakeMoneyClick;
            tableHints.HintClick += OnHintClick;
            boxQuestion.OptionClick += OnOptionClick;

            tableSums.Controls.Add(tableHints);
            tableSums.Controls.Add(buttonTakeMoney);

            Controls.Add(tableSums);
            Controls.Add(dialog);
            Controls.Add(boxAnimation);
            Controls.Add(boxQuestion);
        }

        public void Reset()
        {
            mode = (Mode)Properties.Settings.Default.Mode;

            tableSums.Reset(mode);
            tableHints.Reset(mode);
            dialog.Reset();
            boxQuestion.Reset();
            boxAnimation.Reset();

            buttonTakeMoney.Visible = false;

            SetBoxQuestionVisible(false);
        }

        public async void Start()
        {
            command = mode == Mode.Classic ? SceneCommand.Show_SaveSums : SceneCommand.Show_CountHints;

            await tableSums.MoveX(MainForm.RectScreen.Width - tableSums.Width, 600 / MainForm.DeltaTime);

            tableHints.Visible = true;

            dialog.ButtonCommandEnabled = false;
            dialog.ButtonCommandVisible = true;

            dialog.Text = host.Say(HostPhrases.Rules, tableSums.MaxNumberSum.ToString());

            await tableSums.ShowSums();

            dialog.ButtonCommandEnabled = true;
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
                        command = SceneCommand.NextQuestion;
                    }
                    else
                    {
                        StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
                        buttonTakeMoney.Visible = false;

                        if (boxQuestion.IsCorrectAnswer)
                            command = SceneCommand.Victory;
                        else
                            command = SceneCommand.Loss;
                    }

                    dialog.Text = explanation;
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
                    command = SceneCommand.SwitchQuestion;

                    HostPhrases phrase1 = boxQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestion_CorrectAnswer : HostPhrases.SwitchQuestion_IncorrectAnswer;

                    dialog.Text = $"{explanation}\n{host.Say(phrase1, boxQuestion.Question.Number.ToString())}";
                    break;

                case AnswerMode.TakeMoney:
                    command = SceneCommand.TakeMoney_ShowPrize;

                    HostPhrases phrase2 = boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    dialog.Text = $"{explanation}\n{host.Say(phrase2, tableSums.NextSum.ToString())}";
                    break;
            }

            dialog.ButtonCommandVisible = true;
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
                    command = SceneCommand.End_PhoneFriend;

                    dialog.Reset();
                    dialog.ContentAlignment = ContentAlignment.MiddleLeft;

                    timer = new PhoneTimer((int)(0.3f * dialog.Height));
                    timer.TimeUp += OnCommandClick;

                    await Task.Delay(3000);

                    foreach (var phrase in hint.PhoneFriendDialog(tableSums.NextSum))
                    {
                        dialog.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await dialog.ShowMovingPictureBox(timer, false, 500 / MainForm.DeltaTime);

                    dialog.Text = hint.PhoneFriendAnswer(boxQuestion.Question);
                    timer.Start();

                    dialog.ButtonCommandVisible = true;
                    break;

                case TypeHint.AskAudience:
                    boxQuestion.Enabled = false;
                    command = SceneCommand.End_AskAudience;

                    int heigth = (int)(0.7f * dialog.Height);
                    chart = new VotingChart((int)(0.75f * heigth), heigth);

                    await dialog.ShowMovingPictureBox(chart, true, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, hint.PercentsAudience(boxQuestion.Question));

                    dialog.ButtonCommandVisible = true;
                    break;

                case TypeHint.DoubleDip:
                    boxQuestion.AnswerMode = AnswerMode.DoubleDips;

                    await boxQuestion.ShowCentralIcon(type);
                    break;

                case TypeHint.SwitchQuestion:
                    boxQuestion.AnswerMode = AnswerMode.SwitchQuestion;
                    dialog.Text = host.Say(HostPhrases.SwitchQuestion_AskAnswer);

                    await boxQuestion.ShowCentralIcon(type);
                    break;

                case TypeHint.AskHost:
                    boxQuestion.Enabled = false;
                    dialog.Text = hint.HostAnswer(boxQuestion.Question);

                    await boxQuestion.ShowCentralIcon(type);

                    command = SceneCommand.End_AskHost;
                    dialog.ButtonCommandVisible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            await dialog.RemoveMovingPictureBox(box, countFrames);
            box.Dispose();
        }

        private void SaveSumSelected(int sum)
        {
            dialog.Text = host.Say(HostPhrases.SaveSumSelected, String.Format("{0:#,0}", sum)) + "\n" + host.Say(HostPhrases.GameStart);
            tableSums.SaveSumSelected -= SaveSumSelected;

            command = SceneCommand.Start;
            dialog.ButtonCommandVisible = true;
        }

        private void OnButtonTakeMoneyClick(object sender, EventArgs e)
        {
            ControlEnabled = false;

            command = SceneCommand.TakeMoney_Confirmation;
            cancelCommand = SceneCommand.Cancel_TakingMoney;

            dialog.Text = host.Say(HostPhrases.TakingMoney_ClarifyDecision);
            dialog.ButtonCommandVisible = true;
            dialog.ButtonCancelVisible = true;
        }

        private async Task TransitionToPrize(bool addDelay, bool updatePrize)
        {
            dialog.Clear();
            await boxQuestion.ShowCorrect(addDelay);

            if (updatePrize)
                tableSums.UpdatePrize(boxQuestion.IsCorrectAnswer);

            await boxQuestion.Clear();
            await Task.Delay(500);

            SetBoxQuestionVisible(false);

            await boxAnimation.ShowTransition(boxQuestion.BackgroundImage, prizeImage);
            await boxAnimation.ShowText(tableSums.TextPrize);
        }

        private void SetBoxQuestionVisible(bool visible)
        {
            boxQuestion.Visible = visible;
            boxAnimation.Visible = !visible;
        }

        private async void OnCommandClick(object sender, EventArgs e)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    await TransitionToPrize(false, true);
                    await Task.Delay(3000);
                    await boxAnimation.HideImage();

                    await Task.Delay(3000);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    SetBoxQuestionVisible(true);

                    await boxQuestion.ShowQuestion(boxQuestion.Question.Number + 1);

                    ControlEnabled = true;
                    break;

                case SceneCommand.Loss:
                    await TransitionToPrize(true, true);
                    MessageBox.Show("CONTEXT MENU");
                    break;

                case SceneCommand.Victory:
                    await TransitionToPrize(true, true);
                    MessageBox.Show("CONTEXT MENU");
                    break;

                case SceneCommand.Show_SaveSums:
                    dialog.ButtonCommandEnabled = false;
                    command = SceneCommand.Show_CountHints;

                    dialog.Text = host.Say(HostPhrases.SaveSums, string.Join(", ", Array.ConvertAll(tableSums.SaveSums, x => string.Format("{0:#,0}", x))));

                    await tableSums.ShowSaveSums();

                    dialog.ButtonCommandEnabled = true;
                    break;

                case SceneCommand.Show_CountHints:
                    command = SceneCommand.Show_Hint;
                    dialog.Text = host.Say(HostPhrases.CountHints, tableHints.StringCountActiveHints);
                    break;

                case SceneCommand.Show_Hint:
                    dialog.Text = hint.Description(tableHints.PeekHiddenHint);
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        command = tableHints.CountHints > Hint.MaxCountAllowedHints ? SceneCommand.About_RestrictionsHints : SceneCommand.About_TakingMoney;
                    break;

                case SceneCommand.About_RestrictionsHints:
                    command = SceneCommand.About_TakingMoney;
                    dialog.Text = host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxCountAllowedHints.ToString());
                    break;

                case SceneCommand.About_TakingMoney:
                    command = mode == Mode.Classic ? SceneCommand.About_Starting : SceneCommand.ChoosingSaveSum;
                    dialog.Text = host.Say(HostPhrases.AboutTakingMoney);
                    break;

                case SceneCommand.ChoosingSaveSum:
                    dialog.ButtonCommandVisible = false;
                    dialog.Text = host.Say(HostPhrases.AskSaveSum);
                    tableSums.SaveSumSelected += SaveSumSelected;
                    tableSums.AddSelectionSaveSum();
                    break;

                case SceneCommand.About_Starting:
                    command = SceneCommand.Start;
                    dialog.Text = host.Say(HostPhrases.GameStart);
                    break;

                case SceneCommand.Start:
                    dialog.Clear();
                    tableSums.NumberNextSum = 1;

                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    SetBoxQuestionVisible(true);

                    await boxQuestion.ShowQuestion(1);

                    ControlEnabled = true;
                    buttonTakeMoney.Visible = true;
                    break;

                case SceneCommand.End_PhoneFriend:
                case SceneCommand.End_AskAudience:
                    dialog.Clear();

                    if (command == SceneCommand.End_PhoneFriend)
                    {
                        dialog.ContentAlignment = ContentAlignment.MiddleCenter;

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
                    dialog.Clear();

                    int newIndex;
                    do
                        newIndex = Question.RandomIndex(boxQuestion.Question.Number);
                    while (newIndex == boxQuestion.Question.Index);

                    await boxQuestion.ShowCorrect(true);
                    await Task.Delay(3000);
                    await boxQuestion.Clear();

                    SetBoxQuestionVisible(false);

                    await boxAnimation.HideImage(boxQuestion.BackgroundImage);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    SetBoxQuestionVisible(true);

                    boxQuestion.Reset();

                    await boxQuestion.ShowCentralIcon(TypeHint.SwitchQuestion);
                    await boxQuestion.ShowQuestion(boxQuestion.Question.Number, newIndex);

                    ControlEnabled = true;
                    break;

                case SceneCommand.End_AskHost:
                    dialog.Clear();

                    await boxQuestion.HideCentralIcon();

                    ControlEnabled = true;
                    break;

                case SceneCommand.TakeMoney_Confirmation:
                    command = SceneCommand.TakeMoney;

                    buttonTakeMoney.Visible = false;

                    dialog.Clear();
                    dialog.Text = host.Say(HostPhrases.PlayerTakingMoney, tableSums.TextPrize);
                    dialog.ButtonCommandVisible = true;
                    break;

                case SceneCommand.TakeMoney:
                    dialog.Clear();

                    await Task.Delay(3000);

                    dialog.Text = host.Say(HostPhrases.TakingMoney_AskAnswer);
                    boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.TakeMoney_ShowPrize:
                    await TransitionToPrize(true, false);
                    MessageBox.Show("Context Menu");
                    break;
            }
        }

        private void OnCacnelClick(object sender, EventArgs e)
        {
            switch (cancelCommand)
            {
                case SceneCommand.Cancel_TakingMoney:
                    dialog.Clear();
                    ControlEnabled = true;
                    break;
            }
        }
    }
}
