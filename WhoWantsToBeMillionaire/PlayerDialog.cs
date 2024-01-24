using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PlayerDialog : ControlAnimation
    {
        private readonly Label labelDialog;
        private readonly Bitmap logo;
        private readonly CustomButton buttonCommand;
        private readonly AnswerHint answerHint;

        private VotingChart chart;
        private PhoneTimer timer;

        public new string Text
        {
            set
            {
                labelDialog.Image = null;
                labelDialog.Text = value;
            }
        }

        public ContentAlignment ContentAlignment
        {
            set => labelDialog.TextAlign = value;
        }

        public PlayerDialog(Size size, CustomButton button) : base(size)
        {
            int sideLogo = (int)(0.5f * size.Height);

            labelDialog = new Label();
            logo = new Bitmap(ResourceProcessing.GetImage("Logo.png"), sideLogo, sideLogo);
            buttonCommand = button;
            answerHint = new AnswerHint();

            labelDialog.Size = new Size(size.Width, (int)(0.8f * size.Height));
            labelDialog.BackColor = Color.Transparent;
            labelDialog.Font = new Font("", 0.04f * labelDialog.Size.Height, FontStyle.Bold);
            labelDialog.ForeColor = Color.White;
            labelDialog.TextAlign = ContentAlignment.MiddleCenter;

            buttonCommand.Location = new Point((size.Width - buttonCommand.Width) / 2, (size.Height - labelDialog.Height - buttonCommand.Height) / 2 + labelDialog.Height);
            buttonCommand.Text = "Продолжить";

            Controls.Add(labelDialog);
            Controls.Add(buttonCommand);
        }

        public void Reset()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = null;
            buttonCommand.Visible = false;
            buttonCommand.Enabled = true;
        }

        public void Clear()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = logo;
            buttonCommand.Visible = false;
        }

        public async void OnHintClick(TypeHint type, Question question)
        {
            switch (type)
            {
                case TypeHint.PhoneFriend:
                    timer = new PhoneTimer((int)(0.4f * Height));
                    timer.Location = new Point(Width, 0);

                    labelDialog.Image = null;
                    labelDialog.Controls.Add(timer);
                    //timer.TimeUp += () => buttonNext.PerformClick();

                    await timer.MoveX(Width - timer.Width, 1000 / MainForm.DeltaTime);
                    timer.Start();

                    buttonCommand.Visible = true;
                    break;

                case TypeHint.AskAudience:
                    int heigth = (int)(0.9f * labelDialog.Height);
                    chart = new VotingChart(new Size((int)(0.75f * heigth), heigth));
                    chart.Location = new Point(Width, (labelDialog.Height - heigth) / 2);

                    labelDialog.Image = null;
                    labelDialog.Controls.Add(chart);

                    await chart.MoveX((Width - chart.Width) / 2, 1000 / MainForm.DeltaTime);
                    await Task.Delay(2000);
                    await chart.ShowAnimationVote(3000);
                    await chart.ShowPercents(15, answerHint.GetPersents(question));

                    buttonCommand.Visible = true;
                    break;
            }
        }

        private async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            await box.MoveX(Width, countFrames);
            labelDialog.Controls.Remove(box);
            box.Dispose();
        }

        public async Task RemoveMovingPictureBox()
        {
            int countFrames = 1000 / MainForm.DeltaTime;

            if (labelDialog.Controls.Contains(chart))
                await RemoveMovingPictureBox(chart, countFrames);

            if (labelDialog.Controls.Contains(timer))
                await RemoveMovingPictureBox(timer, countFrames);
        }
    }
}
