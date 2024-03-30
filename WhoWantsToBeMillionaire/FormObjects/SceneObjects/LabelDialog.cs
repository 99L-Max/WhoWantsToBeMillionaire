using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelDialog : Label
    {
        private readonly StringFormat format;

        private int border = 5;
        private Rectangle textRectangle;
        private Rectangle frameRectangle;
        private Image backgroundText;

        public int Border
        {
            set
            {
                border = value;
                SetBorder(value);
                DrawBack();
            }

            get => border;
        }

        public LabelDialog(float sizeFont)
        {
            Dock = DockStyle.Fill;
            Font = new Font("", sizeFont, GraphicsUnit.Pixel);

            textRectangle = frameRectangle = new Rectangle();
            format = new StringFormat();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Text != string.Empty)
            {
                e.Graphics.DrawImage(backgroundText, frameRectangle);
                e.Graphics.DrawString(Text, Font, Brushes.White, textRectangle, format);
            }
            else
            {
                base.OnPaint(e);
            }
        }

        public void SetAlignment(StringAlignment vertical, StringAlignment horizontal)
        {
            format.LineAlignment = vertical;
            format.Alignment = horizontal;
        }

        public void SetRatioText(float ratioWidth, float ratioHeight)
        {
            frameRectangle.Width = (int)(ClientRectangle.Width * ratioWidth);
            frameRectangle.Height = (int)(ClientRectangle.Height * ratioHeight);

            frameRectangle.X = (ClientRectangle.Width - frameRectangle.Width) >> 1;
            frameRectangle.Y = (ClientRectangle.Height - frameRectangle.Height) >> 1;

            SetBorder(border);
            DrawBack();
        }

        private void SetBorder(int border)
        {
            textRectangle.Width = frameRectangle.Width - (border << 1);
            textRectangle.Height = frameRectangle.Height - (border << 1);

            textRectangle.X = frameRectangle.X + border;
            textRectangle.Y = frameRectangle.Y + border;
        }

        private void DrawBack()
        {
            backgroundText?.Dispose();
            backgroundText = new Bitmap(frameRectangle.Width, frameRectangle.Height);

            var frame = new Rectangle(0, 0, frameRectangle.Width, frameRectangle.Height);
            var fill = new Rectangle(border, border, textRectangle.Width, textRectangle.Height);

            using (Graphics g = Graphics.FromImage(backgroundText))
            using (LinearGradientBrush brushFrame = new LinearGradientBrush(frame, Color.Gainsboro, Color.SlateGray, 45f))
            using (LinearGradientBrush brushFill = new LinearGradientBrush(fill, Color.Navy, Color.Black, 90f))
            {
                g.FillRectangle(brushFrame, frame);
                g.FillRectangle(brushFill, fill);
            }

            Invalidate();
        }
    }
}
