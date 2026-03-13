using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class CommandBoard : TableLayoutPanel, IResettable
    {
        private readonly Image _logo;
        private readonly LabelDialog _labelDialog;
        private readonly ButtonWire _buttonCommand;
        private readonly ButtonWire _buttonCanсel;

        private PhoneTimer _phoneTimer;

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

            _buttonCommand.AlignSize();
            _buttonCanсel.AlignSize();
        }

        public event Action<object, SceneCommands> CommandClick;
        public event Action<object, SceneCancelCommands> CancelClick;

        public SceneCommands Command { get; set; }

        public SceneCancelCommands CancelCommand { get; set; }

        public CommandBoardTextModes TextMode { set => SetTextMode(value); }

        public new string Text { set => _labelDialog.Text = value; }

        public bool ButtonCommandVisible { set => _buttonCommand.Visible = value; }

        public bool ButtonCancelVisible { set => _buttonCanсel.Visible = value; }

        public bool ButtonsVisible { set => _buttonCommand.Visible = _buttonCanсel.Visible = value; }

        public bool ButtonCommandEnabled { set => _buttonCommand.Enabled = value; }

        public async Task RemoveMovingControls(int milliseconds)
        {
            foreach (Control ctrl in _labelDialog.Controls)
                if (ctrl is MovingControl moving)
                {
                    await moving.MoveX(_labelDialog.Width, milliseconds / GameConst.DeltaTime);
                    _labelDialog.Controls.Remove(moving);
                    moving.Dispose();
                }
        }

        public async Task CallFriend(Question question, string sum)
        {
            var hint = new PhoneFriendHint();
            var dialog = hint.GetFriendDialog(sum);
            var answerFriend = hint.GetFriendAnswer(question);

            _phoneTimer = new PhoneTimer((int)(0.3f * Height), 30);
            _phoneTimer.X = _labelDialog.Width;
            _phoneTimer.TimeUp += OnTimerTimeUp;

            _labelDialog.Controls.Add(_phoneTimer);

            await Task.Delay(2000);

            Sound.Play(Resources.Hint_PhoneFriend_Beeps);

            await Task.Delay(6000);

            foreach (var phrase in dialog)
            {
                _labelDialog.Text += phrase;
                await Task.Delay(2000);
            }

            var xEnd = _labelDialog.Width - _phoneTimer.Width;
            var countFrames = 500 / GameConst.DeltaTime;

            await _phoneTimer.MoveX(xEnd, countFrames);

            Text = answerFriend;
            _phoneTimer.Start();
        }

        public async Task HoldVote(Question question)
        {
            var percents = new AskAudienceHint().GetPercentagesAudience(question);
            var sizeChart = Resizer.Resize(BasicSizes.Height, (int)(0.7f * Height), 3, 4);
            var chart = new VotingChart(sizeChart);
            var xEnd = _labelDialog.Width - chart.Width >> 1;
            var countFrames = 500 / GameConst.DeltaTime;

            chart.X = _labelDialog.Width;
            chart.Y = _labelDialog.Height - chart.Height >> 1;

            _labelDialog.Image = null;
            _labelDialog.Controls.Add(chart);

            Sound.Play(Resources.Hint_AskAudience_Begin);

            await chart.MoveX(xEnd, countFrames);
            await Task.Delay(3000);
            await chart.ShowAnimationVote(3000);
            await chart.ShowPercents(percents, 15);
        }

        public void StopTimer()
        {
            if (_phoneTimer != null)
            {
                _phoneTimer.TimeUp -= OnTimerTimeUp;
                _phoneTimer.Stop();
                Sound.Play(Resources.Hint_PhoneFriend_End);
            }
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            _labelDialog.Text = string.Empty;
            _labelDialog.Image = null;
            _labelDialog.Controls.Clear();
            _buttonCommand.Visible = _buttonCanсel.Visible = false;

            _buttonCommand.Text = "Продолжить";
            _buttonCanсel.Text = "Пропустить";

            TextMode = CommandBoardTextModes.Monologue;
        }

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
            Command = SceneCommands.TakeMoney_Confirmation;
            CancelCommand = SceneCancelCommands.CancelTakingMoney;

            Text = text;
            ButtonsVisible = true;

            _buttonCommand.Text = "Забрать деньги";
            _buttonCanсel.Text = "Продолжить игру";
        }

        public void AskRestart()
        {
            Command = SceneCommands.Restart;
            CancelCommand = SceneCancelCommands.ExitToMainMenu;

            _buttonCommand.Text = "Повторить игру";
            _buttonCanсel.Text = "Главное меню";

            ButtonsVisible = true;
        }

        private void OnTimerTimeUp(object sender)
        {
            if (sender is PhoneTimer timer)
            {
                timer.TimeUp -= OnTimerTimeUp;
                CommandClick?.Invoke(timer, SceneCommands.End_PhoneFriend);
            }
        }

        private void SetTextMode(CommandBoardTextModes mode)
        {
            if (mode == CommandBoardTextModes.Monologue)
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
}
