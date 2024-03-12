using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameCheckBox : PictureBox, IDisposable
    {
        private readonly Label label;
        private readonly Image imgChecked;
        private readonly Image imgUnchecked;

        private Rectangle rectBox;
        private bool check;

        public bool Checked
        {
            set
            {
                if (check != value)
                {
                    check = value;
                    Invalidate();
                }
            }
            get => check;
        }

        public new string Text
        {
            set => label.Text = value;
            get => label.Text;
        }

        public GameCheckBox(float fontSize)
        {
            Dock = DockStyle.Fill;

            label = new Label();
            label.ForeColor = Color.White;
            label.Font = new Font("", fontSize, FontStyle.Bold);
            label.TextAlign = ContentAlignment.MiddleLeft;

            imgChecked = ResourceManager.GetImage("CheckBox_Checked.png");
            imgUnchecked = ResourceManager.GetImage("CheckBox_Unchecked.png");

            label.MouseDown += OnMouseDown;
            MouseDown += OnMouseDown;
            SizeChanged += OnSizeChanged;

            Controls.Add(label);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            int height = ClientSize.Height / 3;
            rectBox = new Rectangle(height >> 1, (ClientSize.Height - height) >> 1, height, height);

            label.Location = new Point(height << 1, 0);
            label.Size = new Size(ClientSize.Width - (height << 1), ClientSize.Height);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            check = !check;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(check ? imgChecked : imgUnchecked, rectBox);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MouseDown -= OnMouseDown;
                label.MouseDown -= OnMouseDown;
                SizeChanged -= OnSizeChanged;

                imgChecked.Dispose();
                imgUnchecked.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
