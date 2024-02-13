using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum Mode
    {
        Classic,
        Amateur,
        Advanced,
        TEST//УДАЛИТЬ НА РЕЛИЗЕ
    }

    enum SceneCommand
    {
        Start,
        Restart,
        Rules,
        ShowSaveSums,
        ShowCountHints,
        ShowHint,
        AboutTakingMoney,
        WishGoodLuck,
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
        private readonly TableHints tableHints;
        private readonly TableSums tableSums;
        private readonly PictureBoxAnimation containerQuestion;
        private readonly TextQuestion textQuestion;
        private readonly TextPrize textPrize;
        private readonly CustomButton buttonCommand;
        private readonly PlayerDialog playerDialog;
        private readonly AnswerHint answerHint;

        private VotingChart chart;
        private PhoneTimer timer;
        private SceneCommand command = SceneCommand.ShowSaveSums;

        public GameScene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            float widthButton = MainForm.RectScreen.Width * 0.3f;

            buttonCommand = new CustomButton(new Size((int)widthButton, (int)(0.12f * widthButton)));
            tableSums = new TableSums(new Size((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height));
            containerQuestion = new PictureBoxAnimation(new Size(MainForm.RectScreen.Width - tableSums.Width, (int)(MainForm.RectScreen.Height * 0.36f)));
            textQuestion = new TextQuestion(containerQuestion.Size);
            textPrize = new TextPrize(new Size(textQuestion.Width, (int)(0.11f * textQuestion.Width)));
            playerDialog = new PlayerDialog(new Size(MainForm.RectScreen.Width - tableSums.Width, MainForm.RectScreen.Height - textQuestion.Height), buttonCommand);
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
            Controls.Add(playerDialog);
            Controls.Add(containerQuestion);

            Reset();
        }

        private void Reset()
        {
            tableSums.Reset();
            playerDialog.Reset();
            tableHints.Reset();
            textQuestion.Reset();
            textPrize.Reset();
        }

        public async void ShowRules()
        {
            await tableSums.MoveX(MainForm.RectScreen.Width - tableSums.Width, 10);

            buttonCommand.Enabled = false;
            buttonCommand.Visible = true;

            playerDialog.Text =
                $"Вам необходимо правильно ответить на {tableSums.MaxNumberQuestion} вопросов из различных областей знаний. " +
                $"Каждый вопрос имеет 4 варианта ответа, из которых только один является верным.";

            await tableSums.ShowControls();

            buttonCommand.Enabled = true;
        }

        private void OnOptionClick(string explanation)
        {
            tableHints.Enabled = false;

            switch (textQuestion.AnswerMode)
            {
                default:
                    command = SceneCommand.NextQuestion;
                    playerDialog.Text = explanation;
                    break;

                case AnswerMode.SwitchQuestion:
                    command = SceneCommand.SwitchQuestion;
                    playerDialog.Text = answerHint.GetExplanationForSwitchQuestion(textQuestion.Question, textQuestion.IsCorrectAnswer);
                    break;

                case AnswerMode.TakeMoney:
                    command = SceneCommand.TakeMoney;
                    playerDialog.Text = "НЕТ ТЕКСТА";
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

                    playerDialog.Reset();
                    playerDialog.ContentAlignment = ContentAlignment.MiddleLeft;

                    timer = new PhoneTimer((int)(0.3f * playerDialog.Height));
                    timer.TimeUp += OnButtonCommandClick;

                    await Task.Delay(3000);

                    foreach (var phrase in answerHint.GetPhoneFriendDialog(tableSums.NextSum))
                    {
                        playerDialog.AddText(phrase);
                        await Task.Delay(2000);
                    }

                    await playerDialog.ShowMovingPictureBox(timer, false, 500 / MainForm.DeltaTime);

                    playerDialog.Text = answerHint.GetPhoneFriendAnswer(textQuestion.Question);
                    timer.Start();

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.AskAudience:
                    command = SceneCommand.EndAskAudience;

                    int heigth = (int)(0.7f * playerDialog.Height);
                    chart = new VotingChart(new Size((int)(0.75f * heigth), heigth));

                    await playerDialog.ShowMovingPictureBox(chart, true, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, answerHint.GetPersents(textQuestion.Question));

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.SwitchQuestion:
                    playerDialog.Text = answerHint.GetPhraseSwitchQuestion();
                    break;

                case TypeHint.AskHost:
                    command = SceneCommand.EndAskHost;
                    playerDialog.Text = answerHint.GetAskHostAnswer(textQuestion.Question);
                    buttonCommand.Visible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            await playerDialog.RemoveMovingPictureBox(box, countFrames);
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

        private async void OnButtonCommandClick(object sender, EventArgs e)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    playerDialog.Clear();
                    await textQuestion.ShowCorrect(6);

                    if (!textQuestion.IsCorrectAnswer)
                        await Task.Delay(3000);

                    tableSums.AnswerGiven(textQuestion.IsCorrectAnswer);

                    await textQuestion.Clear(6);
                    await ShowPrize(string.Format("{0:#,0}", tableSums.Prize), 15);

                    textQuestion.Reset();

                    if (textQuestion.IsCorrectAnswer)
                    {
                        await Task.Delay(3000);
                        await textPrize.HideText(6);
                        await textPrize.MoveX(textPrize.Width, 15);

                        textPrize.Reset();

                        await Task.Delay(3000);
                        await textQuestion.MoveX(0, 15);
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

                    playerDialog.Text = $"Несгораемые суммы: {string.Join(", ", Array.ConvertAll(tableSums.SaveSums, x => string.Format("{0:#,0}", x)))}.";

                    await tableSums.ShowSaveSums();

                    buttonCommand.Enabled = true;
                    break;

                case SceneCommand.ShowCountHints:
                    command = SceneCommand.ShowHint;
                    playerDialog.Text = $"У Вас есть {tableHints.StringCountActiveHints}.";
                    break;

                case SceneCommand.ShowHint:
                    playerDialog.Text = tableHints.GetDescriptionHint();
                    tableHints.ShowHint();

                    if (tableHints.AllHintsVisible)
                        command = SceneCommand.AboutTakingMoney;
                    break;

                case SceneCommand.AboutTakingMoney:
                    command = SceneCommand.WishGoodLuck;
                    playerDialog.Text = $"До тех пор, пока Вы не дали ответ, можете забрать выигранные деньги.";
                    break;

                case SceneCommand.WishGoodLuck:
                    command = SceneCommand.Start;
                    playerDialog.Text = $"И для Вас начинается игра «Кто хочет стать миллионером?»!!!";
                    break;

                case SceneCommand.Start:
                    playerDialog.Clear();
                    tableSums.NumberQuestion = 1;

                    await textQuestion.MoveX(0, 15);
                    await textQuestion.ShowQuestion(1, 6);

                    tableHints.Enabled = true;
                    break;

                case SceneCommand.EndPhoneFriend:
                case SceneCommand.EndAskAudience:
                    playerDialog.Clear();

                    if (command == SceneCommand.EndPhoneFriend)
                    {
                        playerDialog.ContentAlignment = ContentAlignment.MiddleCenter;

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
                    playerDialog.Clear();

                    int newIndex;
                    do
                        newIndex = Question.RandomIndex(textQuestion.Question.Number);
                    while (newIndex == textQuestion.Question.Index);

                    await textQuestion.ShowCorrect(6);
                    await Task.Delay(3000);
                    await textQuestion.Clear(6);
                    await textQuestion.MoveX(textQuestion.Width, 15);

                    textQuestion.Reset();

                    await textQuestion.MoveX(0, 15);
                    await textQuestion.ShowCentralIcon(TypeHint.SwitchQuestion);
                    await textQuestion.ShowQuestion(textQuestion.Question.Number, newIndex, 6);

                    tableHints.Enabled = true;
                    textQuestion.Enabled = true;
                    break;

                case SceneCommand.EndAskHost:
                    playerDialog.Clear();

                    await textQuestion.HideCentralIcon();

                    textQuestion.Enabled = true;
                    tableHints.Enabled = true;
                    break;
            }
        }
    }
}
