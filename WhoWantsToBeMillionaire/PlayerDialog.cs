using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PlayerDialog : ControlAnimation
    {
        private readonly Label labelDialog;
        private readonly PictureBox logo;
        private readonly CustomButton buttonNext;

        private AudienceChart audience;

        public new string Text
        {
            set
            {
                logo.Visible = false;
                labelDialog.Text = value;
            }
        }

        public PlayerDialog(Size size, CustomButton button) : base(size)
        {
            labelDialog = new Label();
            logo = new PictureBox();
            buttonNext = button;

            labelDialog.Size = new Size(size.Width, (int)(0.8f * size.Height));
            labelDialog.BackColor = Color.Transparent;
            labelDialog.Font = new Font("", 0.04f * labelDialog.Size.Height, FontStyle.Bold);
            labelDialog.ForeColor = Color.White;
            labelDialog.TextAlign = ContentAlignment.MiddleCenter;

            int sideLogo = (int)(0.5f * size.Height);

            logo.BackColor = Color.Transparent;
            logo.BackgroundImageLayout = ImageLayout.Stretch;
            logo.Size = new Size(sideLogo, sideLogo);
            logo.BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Logo.png"), logo.Size);
            logo.Location = new Point((labelDialog.Width - logo.Width) / 2, (labelDialog.Height - logo.Width) / 2);

            buttonNext.Location = new Point((size.Width - buttonNext.Width) / 2, (size.Height - labelDialog.Height - buttonNext.Height) / 2 + labelDialog.Height);
            buttonNext.Text = "Продолжить";

            labelDialog.Controls.Add(logo);
            Controls.Add(labelDialog);
            Controls.Add(buttonNext);
        }

        public void Reset()
        {
            labelDialog.Text = string.Empty;
            logo.Visible = false;
            buttonNext.Visible = false;
            buttonNext.Enabled = true;
        }

        public void Clear()
        {
            labelDialog.Text = string.Empty;
            logo.Visible = true;
            buttonNext.Visible = false;
        }

        public async void OnHintClick(TypeHint type, Question question)
        {
            switch (type)
            {
                case TypeHint.Audience:
                    int heigth = (int)(0.9f * labelDialog.Height);
                    audience = new AudienceChart(new Size((int)(0.75f * heigth), heigth), question);
                    audience.Location = new Point(Width, (labelDialog.Height - heigth) / 2);

                    Controls.Add(audience);
                    labelDialog.Visible = false;

                    int xMiddle = (Width - audience.Width) / 2;
                    int countFrames = 1000 / MainForm.DeltaTime;
                    int dx = (xMiddle - audience.X) / countFrames;

                    do
                    {
                        audience.X += dx;
                        await Task.Delay(MainForm.DeltaTime);
                    } while (--countFrames > 0);

                    audience.X = xMiddle;

                    await Task.Delay(2000);
                    await audience.ShowAnimationVote(3000);
                    await audience.ShowResult(15);

                    buttonNext.Visible = true;
                    break;
            }
        }

        public async Task RemoveMovingPictureBox()
        {
            int countFrames = 1000 / MainForm.DeltaTime;
            int dx;

            if (Controls.Contains(audience))
            {
                dx = (Width - audience.X) / countFrames;

                do
                {
                    audience.X += dx;
                    await Task.Delay(MainForm.DeltaTime);
                } while (--countFrames > 0);

                Controls.Remove(audience);
                audience.Dispose();
            }

            labelDialog.Visible = true;
        }
    }
}
