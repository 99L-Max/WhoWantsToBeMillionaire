using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class CommandBoard : TableLayoutPanel, IReset
    {
        private readonly LabelMenu labelDialog;
        private readonly ButtonWire buttonCommand;
        private readonly ButtonWire buttonCanсel;
        private readonly Bitmap logo;

        public SceneCommand Command;
        public SceneCancelCommand CancelCommand;

        public delegate void EventCommandClick(object sender, SceneCommand command);
        public delegate void EventCancelClick(object sender, SceneCancelCommand command);

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
            set => buttonCanсel.Visible = value;
        }

        public bool ButtonsVisible
        {
            set => buttonCommand.Visible = buttonCanсel.Visible = value;
        }

        public bool ButtonCommandEnabled
        {
            set => buttonCommand.Enabled = value;
        }

        public ContentAlignment ContentAlignment
        {
            set => labelDialog.TextAlign = value;
        }

        public CommandBoard(int width, int height)
        {
            Size = new Size(width, height);

            int sideLogo = (int)(0.6f * height);
            float fontSize = 0.035f * height;

            labelDialog = new LabelMenu(0.032f * height, ContentAlignment.MiddleCenter);
            logo = new Bitmap(ResourceManager.GetImage("Logo.png"), sideLogo, sideLogo);

            buttonCommand = new ButtonWire(fontSize);
            buttonCanсel = new ButtonWire(fontSize);

            buttonCommand.Click += (s, e) => CommandClick.Invoke(this, Command);
            buttonCanсel.Click += (s, e) => CancelClick.Invoke(this, CancelCommand);

            RowCount = 3;

            RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));

            Controls.Add(labelDialog, 0, 0);
            Controls.Add(buttonCommand, 0, 1);
            Controls.Add(buttonCanсel, 0, 2);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = null;
            buttonCommand.Visible = buttonCanсel.Visible = false;

            buttonCommand.Text = "Продолжить";
            buttonCanсel.Text = "Пропустить";
        }

        public void AddText(string text) => labelDialog.Text += text;

        public void Clear()
        {
            labelDialog.Text = string.Empty;
            labelDialog.Image = logo;
            buttonCommand.Visible = buttonCanсel.Visible = false;

            buttonCommand.Text = "Продолжить";
            buttonCanсel.Text = "Отмена";
        }

        public void AskTakingMoney(string text)
        {
            Command = SceneCommand.TakeMoney_Confirmation;
            CancelCommand = SceneCancelCommand.Cancel_TakingMoney;

            Text = text;
            ButtonsVisible = true;

            buttonCommand.Text = "Забрать деньги";
            buttonCanсel.Text = "Продолжить игру";
        }

        public void AskRestart()
        {
            Command = SceneCommand.Restart;
            CancelCommand = SceneCancelCommand.ExitToMainMenu;

            buttonCommand.Text = "Повторить игру";
            buttonCanсel.Text = "Главное меню";

            ButtonsVisible = true;
        }

        public async Task ShowMovingPictureBox(MovingPictureBox box, int milliseconds, bool centering)
        {
            box.Location = new Point(labelDialog.Width, centering ? (labelDialog.Height - box.Height) >> 1 : 0);

            labelDialog.Image = null;
            labelDialog.Controls.Add(box);

            int x = centering ? (labelDialog.Width - box.Width) >> 1 : labelDialog.Width - box.Width;

            await box.MoveX(x, milliseconds / MainForm.DeltaTime);
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
