using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class GameVScrollBar : VScrollBar
    {
        public GameVScrollBar()
        {
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;

            // Отрисовываем бегунок голубого цвета
            int thumbTop = (Value - Minimum) * (rect.Height - LargeChange) / (Maximum - Minimum);
            int thumbHeight = (int)(LargeChange * 1.0f / (Maximum - Minimum) * rect.Height);

            e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(rect.Left, rect.Top + thumbTop, rect.Width, thumbHeight));

            // Отрисовываем остальные элементы, кроме кнопок
            for (int i = 0; i < 2; i++)
            {
                Rectangle buttonRect = new Rectangle(rect.Left, i == 0 ? rect.Top : rect.Bottom - SystemInformation.HorizontalScrollBarHeight, rect.Width, SystemInformation.HorizontalScrollBarHeight);
                ControlPaint.DrawScrollButton(e.Graphics, buttonRect, (i == 0) ? ScrollButton.Up : ScrollButton.Down, ButtonState.Flat);
            }
        }
    }
}
