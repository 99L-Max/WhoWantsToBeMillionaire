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
        private readonly Host host;
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;
        private readonly PictureBoxAnimation containerQuestion;
        private readonly TextQuestion textQuestion;
        private readonly TextPrize textPrize;
        private readonly ButtonWire buttonCommand;
        private readonly PanelDialog dialog;
        private readonly AnswerHint answerHint;

        private VotingChart chart;
        private PhoneTimer timer;
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
            answerHint = new AnswerHint();

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

            dialog.Text =
                $"Вам необходимо правильно ответить на {tableSums.MaxNumberQuestion} вопросов из различных областей знаний. " +
                $"Каждый вопрос имеет 4 варианта ответа, из которых только один является верным.";

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
                    dialog.Text = answerHint.GetExplanationForSwitchQuestion(textQuestion.Question, textQuestion.IsCorrectAnswer);
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

                    foreach (var phrase in answerHint.GetPhoneFriendDialog(tableSums.NextSum))
                    {
                        dialog.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await dialog.ShowMovingPictureBox(timer, false, 500 / MainForm.DeltaTime);

                    dialog.Text = answerHint.GetPhoneFriendAnswer(textQuestion.Question);
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
                    await chart.ShowPercents(15, answerHint.GetPersents(textQuestion.Question));

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.SwitchQuestion:
                    dialog.Text = answerHint.GetPhraseSwitchQuestion();
                    break;

                case TypeHint.AskHost:
                    command = SceneCommand.EndAskHost;
                    dialog.Text = answerHint.GetAskHostAnswer(textQuestion.Question);
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
            dialog.Text = $"{String.Format("{0:#,0}", sum)} рублей — несгораемая сумма!\nИ для Вас начинается игра «Кто хочет стать миллионером?»!!!";
            tableSums.SaveSumSelected -= SaveSumSelected;

            command = SceneCommand.Start;
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

                    dialog.Text = $"Несгораемые суммы: {string.Join(", ", Array.ConvertAll(tableSums.SaveSums, x => string.Format("{0:#,0}", x)))}.";

                    await tableSums.ShowSaveSums();

                    buttonCommand.Enabled = true;
                    break;

                case SceneCommand.ShowCountHints:
                    command = SceneCommand.ShowHint;
                    dialog.Text = $"У Вас есть {tableHints.StringCountActiveHints}.";
                    break;

                case SceneCommand.ShowHint:
                    dialog.Text = tableHints.GetDescriptionHint();
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        command = tableHints.CountActiveHints > TableHints.MaxCountAllowedHints ? SceneCommand.AboutRestrictionsHints : SceneCommand.AboutTakingMoney;
                    break;

                case SceneCommand.AboutRestrictionsHints:
                    command = SceneCommand.AboutTakingMoney;
                    dialog.Text = $"Но использовать можно только {TableHints.MaxCountAllowedHints} из них.";
                    break;

                case SceneCommand.AboutTakingMoney:
                    command = mode == Mode.Classic ? SceneCommand.AboutStarting : SceneCommand.ChoosingSaveSum;
                    dialog.Text = "До тех пор, пока Вы не дали ответ, можете забрать выигранные деньги.";
                    break;

                case SceneCommand.ChoosingSaveSum:
                    buttonCommand.Visible = false;
                    dialog.Text = "Какая сумма будет несгораемой?";
                    tableSums.SaveSumSelected += SaveSumSelected;
                    tableSums.AddSelectionSaveSum();
                    break;

                case SceneCommand.AboutStarting:
                    command = SceneCommand.Start;
                    dialog.Text = "И для Вас начинается игра «Кто хочет стать миллионером?»!!!";
                    break;

                case SceneCommand.Start:
                    dialog.Clear();
                    tableSums.NumberQuestion = 1;

                    await textQuestion.MoveX(0, 20);
                    await textQuestion.ShowQuestion(1, 6);

                    tableHints.Enabled = true;
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
                    dialog.Text = host.AskAfterTakingMoney();
                    textQuestion.Enabled = true;
                    break;
            }
        }
    }
}
