using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum SceneCommand
    {
        Start,
        ShowSaveSums,
        ShowCountHints,
        ShowHint,
        AboutRestrictionsHints,
        AboutTakingMoney,
        ChoosingSaveSum,
        AboutStarting,
        NextQuestion,
        Loss,
        Victory,
        EndPhoneFriend,
        EndAskAudience,
        SwitchQuestion,
        EndAskHost,
        TakeMoney,
    }

    class GameScene : GameContol
    {
        private readonly Bitmap prizeImage;
        private readonly ButtonWire buttonCommand;
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
        private Mode mode;

        public delegate void EventStatisticsChanged(StatsAttribute key, int value = 0);
        public event EventStatisticsChanged StatisticsChanged;

        public GameScene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            host = new Host();
            tableSums = new TableSums((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height);
            boxAnimation = new BoxAnimation(MainForm.RectScreen.Width - tableSums.Width, (int)(MainForm.RectScreen.Height * 0.36f));
            boxQuestion = new BoxQuestion(boxAnimation.Width, boxAnimation.Height);
            buttonCommand = new ButtonWire(MainForm.RectScreen.Width - tableSums.Width, (int)(0.06f * MainForm.RectScreen.Height));
            dialog = new Dialog(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - boxQuestion.Height, buttonCommand);
            tableHints = new TableHints(tableSums.Width, (int)(tableSums.Height * 0.2f));
            hint = new Hint();

            boxAnimation.Location = boxQuestion.Location = new Point(0, MainForm.RectScreen.Height - boxQuestion.Height);

            prizeImage = new Bitmap(boxAnimation.Width, boxAnimation.Height);

            using (Graphics g = Graphics.FromImage(prizeImage))
            using (Image img = ResourceProcessing.GetImage("Question.png"))
            {
                int height = prizeImage.Width * img.Height / img.Width;
                int y = (prizeImage.Height - height) >> 1;
                boxAnimation.SizeFont = 0.42f * height;

                g.DrawImage(img, 0, y, prizeImage.Width, height);
            }

            buttonCommand.Click += OnButtonCommandClick;
            tableHints.HintClick += OnHintClick;
            boxQuestion.OptionClick += OnOptionClick;

            tableSums.Controls.Add(tableHints);

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

            SetBoxQuestionVisible(false);
        }

        public async void Start()
        {
            command = mode == Mode.Classic ? SceneCommand.ShowSaveSums : SceneCommand.ShowCountHints;

            await tableSums.MoveX(MainForm.RectScreen.Width - tableSums.Width, 600 / MainForm.DeltaTime);

            tableHints.Visible = true;

            buttonCommand.Enabled = false;
            buttonCommand.Visible = true;

            dialog.Text = host.Say(HostPhrases.Rules, tableSums.MaxNumberSum.ToString());

            await tableSums.ShowSums();

            buttonCommand.Enabled = true;
        }

        private async void OnOptionClick(Letter letter)
        {
            tableHints.Enabled = false;

            string explanation = boxQuestion.Question.Explanation;
            if (!boxQuestion.IsCorrectAnswer)
                explanation += $"\nПравильный ответ: {boxQuestion.Question.FullCorrect}.";

            switch (boxQuestion.AnswerMode)
            {
                default:
                    if (boxQuestion.IsCorrectAnswer && boxQuestion.Question.Number < tableSums.MaxNumberSum)
                        command = SceneCommand.NextQuestion;
                    else if (boxQuestion.IsCorrectAnswer)
                        command = SceneCommand.Victory;
                    else
                        command = SceneCommand.Loss;

                    StatisticsChanged.Invoke(boxQuestion.IsCorrectAnswer ? StatsAttribute.NumberCorrectAnswers : StatsAttribute.NumberIncorrectAnswers);

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
                    command = SceneCommand.TakeMoney;

                    HostPhrases phrase2 = boxQuestion.IsCorrectAnswer ? HostPhrases.TakingMoney_CorrectAnswer : HostPhrases.TakingMoney_IncorrectAnswer;

                    dialog.Text = $"{explanation}\n{host.Say(phrase2, tableSums.NextSum.ToString())}";
                    break;
            }

            buttonCommand.Visible = true;
        }

        private async void OnHintClick(TypeHint type)
        {
            StatisticsChanged.Invoke(StatsAttribute.NumberHintsUsed);

            tableHints.Enabled = type == TypeHint.FiftyFifty;

            switch (type)
            {
                case TypeHint.FiftyFifty:
                    boxQuestion.SetText(hint.ReduceOptions(boxQuestion.Question));
                    break;

                case TypeHint.PhoneFriend:
                    boxQuestion.Enabled = false;
                    command = SceneCommand.EndPhoneFriend;

                    dialog.Reset();
                    dialog.ContentAlignment = ContentAlignment.MiddleLeft;

                    timer = new PhoneTimer((int)(0.3f * dialog.Height));
                    timer.TimeUp += OnButtonCommandClick;

                    await Task.Delay(3000);

                    foreach (var phrase in hint.PhoneFriendDialog(tableSums.NextSum))
                    {
                        dialog.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await dialog.ShowMovingPictureBox(timer, false, 500 / MainForm.DeltaTime);

                    dialog.Text = hint.PhoneFriendAnswer(boxQuestion.Question);
                    timer.Start();

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.AskAudience:
                    boxQuestion.Enabled = false;
                    command = SceneCommand.EndAskAudience;

                    int heigth = (int)(0.7f * dialog.Height);
                    chart = new VotingChart((int)(0.75f * heigth), heigth);

                    await dialog.ShowMovingPictureBox(chart, true, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, hint.PercentsAudience(boxQuestion.Question));

                    buttonCommand.Visible = true;
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

                    command = SceneCommand.EndAskHost;
                    buttonCommand.Visible = true;
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
            buttonCommand.Visible = true;
        }

        private void PlayerTakingMoney()
        {
            boxQuestion.Enabled = false;
            tableHints.Enabled = false;

            command = SceneCommand.TakeMoney;
            dialog.Text = host.Say(HostPhrases.PlayerTakingMoney, string.Format("{0:#,0}", tableSums.Prize));

            buttonCommand.Visible = true;
        }

        private async Task TransitionToPrize()
        {
            dialog.Clear();
            await boxQuestion.ShowCorrect(!boxQuestion.IsCorrectAnswer);

            tableSums.UpdatePrize(boxQuestion.IsCorrectAnswer);

            await boxQuestion.Clear();
            await Task.Delay(500);

            SetBoxQuestionVisible(false);

            await boxAnimation.ShowTransition(boxQuestion.BackgroundImage, prizeImage);
            await boxAnimation.ShowText(tableSums.TextPrize);

            boxQuestion.Reset();
        }

        private void SetBoxQuestionVisible(bool visible)
        {
            boxQuestion.Visible = visible;
            boxAnimation.Visible = !visible;
        }

        private async void OnButtonCommandClick(object sender, EventArgs e)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    await TransitionToPrize();
                    await Task.Delay(3000);
                    await boxAnimation.HideImage();

                    await Task.Delay(3000);
                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    SetBoxQuestionVisible(true);

                    await boxQuestion.ShowQuestion(boxQuestion.Question.Number + 1);

                    tableHints.Enabled = true;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.Loss:
                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
                    await TransitionToPrize();
                    MessageBox.Show("CONTEXT MENU");
                    break;

                case SceneCommand.Victory:
                    StatisticsChanged.Invoke(StatsAttribute.TotalPrize, tableSums.Prize);
                    await TransitionToPrize();
                    //ПАУЗА
                    MessageBox.Show("CONTEXT MENU");
                    break;

                case SceneCommand.ShowSaveSums:
                    buttonCommand.Enabled = false;
                    command = SceneCommand.ShowCountHints;

                    dialog.Text = host.Say(HostPhrases.SaveSums, string.Join(", ", Array.ConvertAll(tableSums.SaveSums, x => string.Format("{0:#,0}", x))));

                    await tableSums.ShowSaveSums();

                    buttonCommand.Enabled = true;
                    break;

                case SceneCommand.ShowCountHints:
                    command = SceneCommand.ShowHint;
                    dialog.Text = host.Say(HostPhrases.CountHints, tableHints.StringCountActiveHints);
                    break;

                case SceneCommand.ShowHint:
                    dialog.Text = hint.Description(tableHints.PeekHiddenHint);
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        command = tableHints.CountHints > Hint.MaxCountAllowedHints ? SceneCommand.AboutRestrictionsHints : SceneCommand.AboutTakingMoney;
                    break;

                case SceneCommand.AboutRestrictionsHints:
                    command = SceneCommand.AboutTakingMoney;
                    dialog.Text = host.Say(HostPhrases.AboutRestrictionsHints, Hint.MaxCountAllowedHints.ToString());
                    break;

                case SceneCommand.AboutTakingMoney:
                    command = mode == Mode.Classic ? SceneCommand.AboutStarting : SceneCommand.ChoosingSaveSum;
                    dialog.Text = host.Say(HostPhrases.AboutTakingMoney);
                    break;

                case SceneCommand.ChoosingSaveSum:
                    buttonCommand.Visible = false;
                    dialog.Text = host.Say(HostPhrases.AskSaveSum);
                    tableSums.SaveSumSelected += SaveSumSelected;
                    tableSums.AddSelectionSaveSum();
                    break;

                case SceneCommand.AboutStarting:
                    command = SceneCommand.Start;
                    dialog.Text = host.Say(HostPhrases.GameStart);
                    break;

                case SceneCommand.Start:
                    dialog.Clear();
                    tableSums.NumberNextSum = 1;

                    await boxAnimation.ShowImage(boxQuestion.BackgroundImage);

                    SetBoxQuestionVisible(true);

                    await boxQuestion.ShowQuestion(1);

                    tableHints.Enabled = true;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.EndPhoneFriend:
                case SceneCommand.EndAskAudience:
                    dialog.Clear();

                    if (command == SceneCommand.EndPhoneFriend)
                    {
                        dialog.ContentAlignment = ContentAlignment.MiddleCenter;

                        timer.Stop();
                        timer.TimeUp -= OnButtonCommandClick;

                        await Task.Delay(2000);
                        await RemoveMovingPictureBox(timer, 500 / MainForm.DeltaTime);
                    }
                    else
                    {
                        await RemoveMovingPictureBox(chart, 1000 / MainForm.DeltaTime);
                    }

                    boxQuestion.Enabled = true;
                    tableHints.Enabled = true;
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

                    tableHints.Enabled = true;
                    boxQuestion.Enabled = true;
                    break;

                case SceneCommand.EndAskHost:
                    dialog.Clear();

                    await boxQuestion.HideCentralIcon();

                    boxQuestion.Enabled = true;
                    tableHints.Enabled = true;
                    break;

                case SceneCommand.TakeMoney:
                    dialog.Clear();

                    await Task.Delay(3000);

                    dialog.Text = host.Say(HostPhrases.TakingMoney_AskAnswer);
                    boxQuestion.AnswerMode = AnswerMode.TakeMoney;
                    boxQuestion.Enabled = true;
                    break;
            }
        }
    }
}
