using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    enum TextMode { Monologue, Dialog }

    class CommandBoard : TableLayoutPanel, IReset
    {
        private readonly LabelDialog _labelDialog;
        private readonly ButtonWire _buttonCommand;
        private readonly ButtonWire _buttonCanсel;
        private readonly Image _logo;

        public SceneCommand Command;
        public SceneCancelCommand CancelCommand;

        public Action<object, SceneCommand> CommandClick;
        public Action<object, SceneCancelCommand> CancelClick;

        public CommandBoard(int width, int height)
        {
            Size = new Size(width, height);
            BackColor = Color.Transparent;

            int sideLogo = (int)(0.6f * height);

            _labelDialog = new LabelDialog(0.04f * height);
            _logo = new Bitmap(Resources.Logo, sideLogo, sideLogo);

            _buttonCommand = new ButtonWire();
            _buttonCanсel = new ButtonWire();

            _buttonCommand.Click += (s, e) => CommandClick?.Invoke(this, Command);
            _buttonCanсel.Click += (s, e) => CancelClick?.Invoke(this, CancelCommand);

            RowCount = 3;

            RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));
            RowStyles.Add(new RowStyle(SizeType.Percent, 1f));

            Controls.Add(_labelDialog, 0, 0);
            Controls.Add(_buttonCommand, 0, 1);
            Controls.Add(_buttonCanсel, 0, 2);
        }

        public TextMode TextMode
        {
            set
            {
                if (value == TextMode.Monologue)
                {
                    _labelDialog.SetRatioTextArea(0.8f, 0.8f);
                    _labelDialog.SetAlignment(StringAlignment.Center, StringAlignment.Center);
                }
                else
                {
                    _labelDialog.SetRatioTextArea(0.65f, 0.9f);
                    _labelDialog.SetAlignment(StringAlignment.Center, StringAlignment.Near);
                }
            }
        }

        public new string Text { set => _labelDialog.Text = value; }

        public bool ButtonCommandVisible { set => _buttonCommand.Visible = value; }

        public bool ButtonCancelVisible { set => _buttonCanсel.Visible = value; }

        public bool ButtonsVisible { set => _buttonCommand.Visible = _buttonCanсel.Visible = value; }

        public bool ButtonCommandEnabled { set => _buttonCommand.Enabled = value; }

        public void Reset(Mode mode = Mode.Classic)
        {
            _labelDialog.Text = string.Empty;
            _labelDialog.Image = null;
            _labelDialog.Controls.Clear();
            _buttonCommand.Visible = _buttonCanсel.Visible = false;

            _buttonCommand.Text = "Продолжить";
            _buttonCanсel.Text = "Пропустить";

            TextMode = TextMode.Monologue;
        }

        public void AddText(string text) =>
            _labelDialog.Text += text;

        public void Clear()
        {
            _labelDialog.Text = string.Empty;
            _labelDialog.Image = _logo;
            _buttonCommand.Visible = _buttonCanсel.Visible = false;

            _buttonCommand.Text = "Продолжить";
            _buttonCanсel.Text = "Отмена";
        }

        public void AskTakingMoney(string text)
        {
            Command = SceneCommand.TakeMoney_Confirmation;
            CancelCommand = SceneCancelCommand.CancelTakingMoney;

            Text = text;
            ButtonsVisible = true;

            _buttonCommand.Text = "Забрать деньги";
            _buttonCanсel.Text = "Продолжить игру";
        }

        public void AskRestart()
        {
            Command = SceneCommand.Restart;
            CancelCommand = SceneCancelCommand.ExitToMainMenu;

            _buttonCommand.Text = "Повторить игру";
            _buttonCanсel.Text = "Главное меню";

            ButtonsVisible = true;
        }

        public async Task ShowMovingControl(MovingControl box, int milliseconds, bool centering)
        {
            box.Location = new Point(_labelDialog.Width, centering ? _labelDialog.Height - box.Height >> 1 : 0);

            _labelDialog.Image = null;
            _labelDialog.Controls.Add(box);

            int x = centering ? _labelDialog.Width - box.Width >> 1 : _labelDialog.Width - box.Width;

            await box.MoveX(x, milliseconds / MainForm.DeltaTime);
        }

        public async Task RemoveMovingControls(MovingControl box, int countFrames)
        {
            if (_labelDialog.Controls.Contains(box))
            {
                await box.MoveX(_labelDialog.Width, countFrames);
                _labelDialog.Controls.Remove(box);
            }
        }
    }
}
