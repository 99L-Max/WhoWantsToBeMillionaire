﻿using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class MovingTableControls : MovingControl, IReset, ISetSettings
    {
        private float _percentageHeight = 0f;

        public MovingTableControls(int width, int height) : base(width, height)
        {
            BackgroundImage = new Bitmap(Resources.Background_Sums, width, height);
        }

        public void Add(Control ctrl, float percentHeight, float ratioWith, float ratioHeight)
        {
            var cellHeight = (int)(percentHeight / 100f * Height);
            var yCell = (int)(_percentageHeight / 100f * Height);
            var height = (int)(ratioHeight * cellHeight);
            var width = (int)(ratioWith * Width);

            _percentageHeight += percentHeight;

            ctrl.Size = new Size(width, height);
            ctrl.Location = new Point(Width - width >> 1, (cellHeight - height >> 1) + yCell);

            Controls.Add(ctrl);
        }

        public void Reset(Mode mode = Mode.Classic)
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.Visible = false;

                if (ctrl is IReset res)
                    res.Reset(mode);
            }

            X = GameConst.ScreenSize.Width;
        }

        public void SetSettings(SettingsData data)
        {
            foreach (Control ctrl in Controls)
                if (ctrl is ISetSettings set)
                    set.SetSettings(data);
        }
    }
}
