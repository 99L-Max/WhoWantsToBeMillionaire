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
        EndCall,
        EndAudience,
        ReplaceQuestion,
        EndHost,
        TakeMoney,
    }

    class GameScene : ControlAnimation
    {
        private readonly ContainerHints containerHints;
        private readonly ContainerSums containerSums;
        private readonly ContainerQuestion containerQuestion;
        private readonly CustomButton buttonCommand;
        private readonly PlayerDialog playerDialog;

        private SceneCommand command = SceneCommand.ShowSaveSums;

        public GameScene() : base(MainForm.RectScreen.Size)
        {
            Dock = DockStyle.Fill;

            float widthButton = MainForm.RectScreen.Width * 0.3f;

            buttonCommand = new CustomButton(new Size((int)widthButton, (int)(0.12f * widthButton)));
            containerSums = new ContainerSums(new Size((int)(MainForm.RectScreen.Width * 0.3f), MainForm.RectScreen.Height));
            containerQuestion = new ContainerQuestion(new Size(MainForm.RectScreen.Width - containerSums.Width, (int)(MainForm.RectScreen.Height * 0.36f)));
            playerDialog = new PlayerDialog(new Size(MainForm.RectScreen.Width - containerSums.Width, MainForm.RectScreen.Height - containerQuestion.Height), buttonCommand);
            containerHints = new ContainerHints(new Size(containerSums.Width, (int)(containerSums.Height * 0.2f)));

            containerQuestion.Location = new Point(0, MainForm.RectScreen.Height - containerQuestion.Height);

            buttonCommand.Click += OnButtonCommandClick;
            containerHints.HintClick += OnHintClick;
            containerQuestion.OptionClick += OnOptionClick;

            containerSums.Controls.Add(containerHints);

            Controls.Add(containerSums);
            Controls.Add(containerQuestion);
            Controls.Add(playerDialog);

            Reset();
        }

        private void Reset()
        {
            containerSums.Reset();
            playerDialog.Reset();
            containerHints.Reset();
        }

        public async void Start()
        {
            await containerSums.Show();

            buttonCommand.Enabled = false;
            buttonCommand.Visible = true;

            playerDialog.Text =
                $"Вам необходимо правильно ответить на {containerSums.MaxNumberQuestion} вопросов из различных областей знаний. " +
                $"Каждый вопрос имеет 4 варианта ответа, из которых только один является верным.";

            await containerSums.ShowControls();

            buttonCommand.Enabled = true;
        }

        private void OnOptionClick(string explanation)
        {
            containerHints.Enabled = false;
            playerDialog.Text = explanation;
            buttonCommand.Visible = true;

            switch (containerQuestion.AnswerMode)
            {
                default:
                    command = SceneCommand.NextQuestion;
                    break;
                case AnswerMode.ReplaceQuestion:
                    command = SceneCommand.ReplaceQuestion;
                    break;
                case AnswerMode.TakingMoney:
                    command = SceneCommand.TakeMoney;
                    break;
            }
        }

        private void OnHintClick(TypeHint type)
        {
            switch (type)
            {
                case TypeHint.Call:
                    command = SceneCommand.EndCall;
                    break;
                case TypeHint.Audience:
                    command = SceneCommand.EndAudience;
                    break;
                case TypeHint.Replace:
                    command = SceneCommand.ReplaceQuestion;
                    break;
                case TypeHint.Host:
                    command = SceneCommand.EndHost;
                    break;
            }

            containerHints.Enabled = type == TypeHint.FiftyFifty;
            playerDialog.OnHintClick(type, containerQuestion.Question);
            containerQuestion.OnHintClick(type);
        }

        private async void OnButtonCommandClick(object sender, EventArgs e)
        {
            switch (command)
            {
                case SceneCommand.NextQuestion:
                    playerDialog.Clear();
                    await containerQuestion.ShowCorrect();

                    if (!containerQuestion.IsCorrectAnswer)
                        await Task.Delay(3000);

                    containerSums.AnswerGiven(containerQuestion.IsCorrectAnswer);
                    await containerQuestion.ShowPrize(string.Format("{0:#,0}", containerSums.Prize));

                    if (containerQuestion.IsCorrectAnswer)
                    {
                        await Task.Delay(3000);
                        await containerQuestion.HidePrize();
                        await Task.Delay(3000);
                        await containerQuestion.ShowQuestion(containerSums.NumberQuestion);
                        containerHints.Enabled = true;
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

                    playerDialog.Text = $"Несгораемые суммы: {string.Join(", ", Array.ConvertAll(containerSums.SaveSums, x => string.Format("{0:#,0}", x)))}.";

                    await containerSums.ShowSaveSums();

                    buttonCommand.Enabled = true;
                    break;

                case SceneCommand.ShowCountHints:
                    command = SceneCommand.ShowHint;
                    playerDialog.Text = $"У Вас есть {containerHints.StringCountActiveHints}.";
                    break;

                case SceneCommand.ShowHint:
                    playerDialog.Text = containerHints.GetDescriptionHint();
                    containerHints.ShowHint();

                    if (containerHints.AllHintsVisible)
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
                    containerSums.NumberQuestion = 1;
                    await containerQuestion.ShowQuestion(1);
                    containerHints.Enabled = true;
                    break;

                case SceneCommand.ReplaceQuestion:
                    playerDialog.Clear();
                    await containerQuestion.ReplaceQuestion();
                    containerHints.Enabled = true;
                    break;
            }
        }
    }
}
