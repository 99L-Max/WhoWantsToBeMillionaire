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
        ShowSaveSums,
        ShowCountHints,
        ShowHint,
        AboutRestrictionsHints,
        AboutTakingMoney,
        ChoosingSaveSum,
        AboutStarting,
        NextQuestion,
        ShowLoss,
        EndPhoneFriend,
        EndAskAudience,
        SwitchQuestion,
        EndAskHost,
        TakeMoney,
    }

    class GameScene : PictureBoxAnimation
    {
        private readonly ButtonWire buttonCommand;
        private readonly Host host;
        private readonly Hint hint;
        private readonly PanelDialog dialog;
        private readonly PictureBoxAnimation containerQuestion;
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;
        private readonly TextPrize textPrize;
        private readonly TextQuestion textQuestion;

        private PhoneTimer timer;
        private VotingChart chart;
        private SceneCommand command;
        private Mode mode;

        public GameScene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            host = new Host();
            tableSums = new TableSums(new Size((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height));
            containerQuestion = new PictureBoxAnimation(new Size(MainForm.RectScreen.Width - tableSums.Width, (int)(MainForm.RectScreen.Height * 0.36f)));
            textQuestion = new TextQuestion(containerQuestion.Size);
            textPrize = new TextPrize(new Size(textQuestion.Width, (int)(0.11f * textQuestion.Width)));
            buttonCommand = new ButtonWire(new Size(MainForm.RectScreen.Width - tableSums.Width, (int)(0.06f * MainForm.RectScreen.Height)));
            dialog = new PanelDialog(new Size(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - textQuestion.Height), buttonCommand);
            tableHints = new TableHints(new Size(tableSums.Width, (int)(tableSums.Height * 0.2f)));
            hint = new Hint();

            containerQuestion.Location = new Point(0, MainForm.RectScreen.Height - textQuestion.Height);

            textPrize.Y = (textQuestion.Height - textPrize.Height) / 2;

            buttonCommand.Click += OnButtonCommandClick;
            tableHints.HintClick += textQuestion.OnHintClick;
            tableHints.HintClick += OnHintClick;
            textQuestion.OptionClick += OnOptionClick;

            tableSums.Controls.Add(tableHints);

            containerQuestion.Controls.Add(textPrize);
            containerQuestion.Controls.Add(textQuestion);

            Controls.Add(tableSums);
            Controls.Add(dialog);
            Controls.Add(containerQuestion);
        }

        public void Reset()
        {
            mode = (Mode)Properties.Settings.Default.Mode;

            tableSums.Reset(mode);
            tableHints.Reset(mode);
            dialog.Reset();
            textQuestion.Reset();
            textPrize.Reset();
        }

        public async void Start()
        {
            command = mode == Mode.Classic ? SceneCommand.ShowSaveSums : SceneCommand.ShowCountHints;

            await tableSums.MoveX(MainForm.RectScreen.Width - tableSums.Width, 15);

            tableHints.Visible = true;

            buttonCommand.Enabled = false;
            buttonCommand.Visible = true;

            dialog.Text = host.Say(HostPhrases.Rules, tableSums.MaxNumberQuestion.ToString());

            await tableSums.ShowSums();

            buttonCommand.Enabled = true;
        }

        private void OnOptionClick(string explanation)
        {
            tableHints.Enabled = false;

            switch (textQuestion.AnswerMode)
            {
                default:
                    command = SceneCommand.NextQuestion;
                    dialog.Text = explanation;
                    break;

                case AnswerMode.SwitchQuestion:
                    command = SceneCommand.SwitchQuestion;
                    HostPhrases phrase = textQuestion.IsCorrectAnswer ? HostPhrases.SwitchQuestionCorrect : HostPhrases.SwitchQuestionIncorrect;
                    dialog.Text = $"{explanation}\n{host.Say(phrase, textQuestion.Question.Number.ToString())}";
                    break;

                case AnswerMode.TakeMoney:
                    command = SceneCommand.TakeMoney;
                    dialog.Text = "ДОБАВИТЬ ТЕКСТ";
                    break;
            }

            buttonCommand.Visible = true;
        }

        private async void OnHintClick(TypeHint type)
        {
            tableHints.Enabled = type == TypeHint.FiftyFifty;

            switch (type)
            {
                case TypeHint.PhoneFriend:
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

                    dialog.Text = hint.PhoneFriendAnswer(textQuestion.Question);
                    timer.Start();

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.AskAudience:
                    command = SceneCommand.EndAskAudience;

                    int heigth = (int)(0.7f * dialog.Height);
                    chart = new VotingChart(new Size((int)(0.75f * heigth), heigth));

                    await dialog.ShowMovingPictureBox(chart, true, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, hint.PercentsAudience(textQuestion.Question));

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.SwitchQuestion:
                    dialog.Text = host.Say(HostPhrases.AskBeforeSwitchQuestion);
                    break;

                case TypeHint.AskHost:
                    command = SceneCommand.EndAskHost;
                    dialog.Text = hint.HostAnswer(textQuestion.Question);
                    buttonCommand.Visible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            await dialog.RemoveMovingPictureBox(box, countFrames);
            box.Dispose();
        }

        private async Task ShowPrize(string text, int countFrames)
        {
            int dx = -textPrize.X / countFrames;

            do
            {
                textPrize.X += dx;
                textQuestion.X += dx;
                await Task.Delay(MainForm.DeltaTime);
            } while (--countFrames > 0);

            textPrize.X = 0;

            await textPrize.ShowText(text, 6);
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
            textQuestion.Enabled = false;
            tableHints.Enabled = false;

            command = SceneCommand.TakeMoney;
            dialog.Text = host.Say(HostPhrases.PlayerTakingMoney, tableSums.Prize.ToString());

            buttonCommand.Visible = true;
        }

        private async void OnButtonCommandClick(object sender, EventArgs e)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    dialog.Clear();
                    await textQuestion.ShowCorrect(6);

                    if (!textQuestion.IsCorrectAnswer)
                        await Task.Delay(3000);

                    tableSums.AnswerGiven(textQuestion.IsCorrectAnswer);

                    await textQuestion.Clear(6);
                    await Task.Delay(500);
                    await ShowPrize(string.Format("{0:#,0}", tableSums.Prize), 20);

                    textQuestion.Reset();

                    if (textQuestion.IsCorrectAnswer)
                    {
                        await Task.Delay(3000);
                        await textPrize.HideText(6);
                        await textPrize.MoveX(textPrize.Width, 20);

                        textPrize.Reset();

                        await Task.Delay(3000);
                        await textQuestion.MoveX(0, 20);
                        await textQuestion.ShowQuestion(tableSums.NumberQuestion, 6);

                        tableHints.Enabled = true;
                        textQuestion.Enabled = true;
                    }
                    else
                    {
                        command = SceneCommand.Restart;
                        buttonCommand.Text = "Новая игра";
                        buttonCommand.Visible = true;
                    }

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
                    tableSums.NumberQuestion = 1;

                    await textQuestion.MoveX(0, 20);
                    await textQuestion.ShowQuestion(1, 6);

                    tableHints.Enabled = true;
                    textQuestion.Enabled = true;
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

                    textQuestion.Enabled = true;
                    tableHints.Enabled = true;
                    break;

                case SceneCommand.SwitchQuestion:
                    dialog.Clear();

                    int newIndex;
                    do
                        newIndex = Question.RandomIndex(textQuestion.Question.Number);
                    while (newIndex == textQuestion.Question.Index);

                    await textQuestion.ShowCorrect(6);
                    await Task.Delay(3000);
                    await textQuestion.Clear(6);
                    await textQuestion.MoveX(textQuestion.Width, 20);

                    textQuestion.Reset();

                    await textQuestion.MoveX(0, 20);
                    await textQuestion.ShowCentralIcon(TypeHint.SwitchQuestion);
                    await textQuestion.ShowQuestion(textQuestion.Question.Number, newIndex, 6);

                    tableHints.Enabled = true;
                    textQuestion.Enabled = true;
                    break;

                case SceneCommand.EndAskHost:
                    dialog.Clear();

                    await textQuestion.HideCentralIcon();

                    textQuestion.Enabled = true;
                    tableHints.Enabled = true;
                    break;

                case SceneCommand.TakeMoney:
                    dialog.Clear();

                    await Task.Delay(3000);

                    dialog.Text = host.Say(HostPhrases.AskAfterTakingMoney);
                    textQuestion.ModeTakeMoney();
                    textQuestion.Enabled = true;
                    break;
            }
        }
    }
}
