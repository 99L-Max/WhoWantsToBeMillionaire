using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MovingTableControls : MovingControl, IReset, IGameSettings
    {
        private float _percentsHeights = 0f;

        public MovingTableControls(int width, int height) : base(width, height)
        {
            BackgroundImage = new Bitmap(Resources.Background_Sums, width, height);
        }

        public void Add(Control ctrl, float percentHeight, float ratioWith, float ratioHeight)
        {
            var cellHeight = (int)(percentHeight / 100f * Height);
            var yCell = (int)(_percentsHeights / 100f * Height);
            var height = (int)(ratioHeight * cellHeight);
            var width = (int)(ratioWith * Width);

            _percentsHeights += percentHeight;

            ctrl.Size = new Size(width, height);
            ctrl.Location = new Point(Width - width >> 1, (cellHeight - height >> 1) + yCell);

            Controls.Add(ctrl);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            foreach (Control ctrl in Controls)
            { 
                ctrl.Visible = false;

                if (ctrl is IReset)
                    (ctrl as IReset).Reset(mode);
            }

            X = MainForm.ScreenRectangle.Width;
        }

        public void SetSettings(GameSettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is IGameSettings)
                    (ctrl as IGameSettings).SetSettings(data);
        }
    }
}
