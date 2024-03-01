using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class Dialog : TableLayoutPanel
    {
        private readonly LabelMenu labelDialog;
        private readonly ButtonWire buttonCommand;
        private readonly ButtonWire buttonCansel;
        private readonly Bitmap logo;

        public delegate void EventCommandClick(object sender, EventArgs e);
        public delegate void EventCancelClick(object sender, EventArgs e);

        public event EventCommandClick CommandClick;
        public event EventCancelClick CancelClick;

        public new string Text
        {
            set
            {
                labelDialog.Image = null;
                labelDialog.Text = value;
            }
        }

        public bool ButtonCommandVisible
        {
            set => buttonCommand.Visible = value;
        }

        public bool ButtonCancelVisible
        {
            set => buttonCansel.Visible = value;
        }

        public bool ButtonCommandEnabled
        {
            set => buttonCommand.Enabled = value;
        }

        public ContentAlignment ContentAlignment
        {
            set => labelDialog.TextAlign = value;
        }

        public Dialog(int width, int height)
        {
            Size = new Size(width, height);

            int sideLogo = (int)(0.6f * height);
            float fontSize = 0.035f * height;

            labelDialog = new LabelMenu(0.032f * height, ContentAlignment.MiddleCenter);
            logo = new Bitmap(ResourceProcessing.GetImage("Logo.png"), sideLogo, sideLogo);

            buttonCommand = new ButtonWire(fontSize);
            buttonCansel = new ButtonWire(fontSize);

            buttonCommand.Click += (s, e) => CommandClick.Invoke(this, e);
            buttonCansel.Click += (s, e) => CancelClick.Invoke(this, e);

            buttonCommand.Text = "Продолжить";
            buttonCansel.Text = "Отмена";

            RowCount = 3;

            RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));

            Controls.Add(labelDialog, 0, 0);
            Controls.Add(buttonCommand, 0, 1);
            Controls.Add(buttonCansel, 0, 2);
        }

        public void Reset()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = null;
            buttonCommand.Visible = buttonCansel.Visible = false;
        }

        public void AddText(string text) => labelDialog.Text += text;

        public void Clear()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = logo;
            buttonCommand.Visible = buttonCansel.Visible = false;
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
