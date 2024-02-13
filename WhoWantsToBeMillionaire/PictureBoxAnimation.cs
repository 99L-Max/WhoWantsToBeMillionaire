using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class PictureBoxAnimation : PictureBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                base.CreateParams.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public PictureBoxAnimation(Size size)
        {
            Size = size;
            BackColor = Color.Transparent;
        }
    }
}
