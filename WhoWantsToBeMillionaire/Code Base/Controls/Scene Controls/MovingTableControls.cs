using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MovingTableControls : MovingControl, IResettable, ISettable
    {
        private float _percentageHeight = 0f;

        public MovingTableControls(int width, int height) : base(width, height)
        {
            BackgroundImage = new Bitmap(Resources.Background_Sums, width, height);
        }

        public void Add(Control ctrl, float heightPercent, float ratioWidth, float ratioHeight)
        {
            var cellHeight = (int)(heightPercent / GameConst.MaxPercent * Height);
            var yCell = (int)(_percentageHeight / GameConst.MaxPercent * Height);
            var height = (int)(ratioHeight * cellHeight);
            var width = (int)(ratioWidth * Width);

            _percentageHeight += heightPercent;

            ctrl.Size = new Size(width, height);
            ctrl.Location = new Point(Width - width >> 1, (cellHeight - height >> 1) + yCell);

            Controls.Add(ctrl);
        }

        public void Reset(Modes mode = Modes.Classic)
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.Visible = false;

                if (ctrl is IResettable res)
                    res.Reset(mode);
            }

            X = GameConst.ScreenSize.Width;
        }

        public void SetSettings(SettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is ISettable set)
                    set.SetSettings(data);
        }
    }
}
