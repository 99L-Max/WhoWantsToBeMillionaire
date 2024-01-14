using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PlayerDialog : ControlAnimation
    {
        private readonly Label labelDialog;
        private readonly PictureBox logo;
        private readonly CustomButton buttonNext;

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
            labelDialog.Controls.Add(logo);

            int sideLogo = (int)(0.5f * size.Height);

            logo.BackColor = Color.Transparent;
            logo.BackgroundImageLayout = ImageLayout.Stretch;
            logo.Size = new Size(sideLogo, sideLogo);
            logo.BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Logo.png"), logo.Size);
            logo.Location = new Point((labelDialog.Width - logo.Width) / 2, (labelDialog.Height - logo.Width) / 2);

            buttonNext.Location = new Point((size.Width - buttonNext.Width) / 2, (size.Height - labelDialog.Height - buttonNext.Height) / 2 + labelDialog.Height);
            buttonNext.Text = "Продолжить";

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

        public void OnHintClick(TypeHint type, Question question)
        {

        }
    }
}
