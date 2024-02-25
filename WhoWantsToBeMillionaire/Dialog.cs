using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class Dialog : GameContol
    {
        private readonly Label labelDialog;
        private readonly Bitmap logo;
        private readonly ButtonWire buttonCommand;

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

        public Dialog(int width, int height, ButtonWire button) : base(width, height)
        {
            int sideLogo = (int)(0.5f * height);

            labelDialog = new Label();
            logo = new Bitmap(ResourceProcessing.GetImage("Logo.png"), sideLogo, sideLogo);
            buttonCommand = button;

            labelDialog.Size = new Size(width, (int)(0.8f * height));
            labelDialog.BackColor = Color.Transparent;
            labelDialog.Font = new Font("", 0.04f * labelDialog.Size.Height, FontStyle.Bold);
            labelDialog.ForeColor = Color.White;
            labelDialog.TextAlign = ContentAlignment.MiddleCenter;

            buttonCommand.Location = new Point((width - buttonCommand.Width) / 2, (height - labelDialog.Height - buttonCommand.Height) / 2 + labelDialog.Height);
            buttonCommand.Text = "Продолжить";

            Controls.Add(labelDialog);
            Controls.Add(buttonCommand);
        }

        public void Reset()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = null;
            buttonCommand.Visible = false;
        }

        public void AddText(string text) => labelDialog.Text += text;

        public void Clear()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = logo;
            buttonCommand.Visible = false;
        }

        public async Task ShowMovingPictureBox(MovingPictureBox box, bool isCenterHeight, int countFrames)
        {
            box.Location = new Point(labelDialog.Width, isCenterHeight ? (labelDialog.Height - box.Height) / 2 : 0);

            labelDialog.Image = null;
            labelDialog.Controls.Add(box);

            await box.MoveX(isCenterHeight ? (labelDialog.Width - box.Width) / 2 : labelDialog.Width - box.Width, countFrames);
        }

        public async Task RemoveMovingPictureBox(MovingPictureBox box, int countFrames)
        {
            if (labelDialog.Controls.Contains(box))
            {
                await box.MoveX(labelDialog.Width, countFrames);
                labelDialog.Controls.Remove(box);
            }
        }
    }
}
