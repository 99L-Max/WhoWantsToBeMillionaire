using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class Option : TextPictureBox
    {
        private readonly RectangleF rectangle;
        private readonly RectangleF rectLabel;
        private readonly StringFormat formatLabel;
        private readonly string label;

        private Color foreColorLetter;

        public readonly Letter Letter;

        public Option(Letter letter, Size size, Bitmap image, float fontSize, StringFormat format) : base(size, image, fontSize, format)
        {
            Letter = letter;
            label = $"{letter}:";
            rectLabel = new RectangleF(0, 0, rectangle.X, rectangle.Height);

            formatLabel = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };

            rectLabel = new RectangleF(0f, 0f, 0.15f * Width, Height);
            rectangle = new RectangleF(rectLabel.Width, 0f, Width, Height);

            Reset();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
           e.Graphics.DrawImage(BackgroundImage, ClientRectangle);
           
           if (Image != null)
               e.Graphics.DrawImage(Image, ClientRectangle);

            if (alpha > 0)
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, ForeColor)))
                using (Brush brushLetter = new SolidBrush(Color.FromArgb(alpha, foreColorLetter)))
                {
                    e.Graphics.DrawString(Text, Font, brush, rectangle, format);
                    e.Graphics.DrawString(label, Font, brushLetter, rectLabel, formatLabel);
                }
        }

        public void Reset()
        {
            Image = null;
            SetForeColors(Color.White, Color.Orange);
        }

        private void SetForeColors(Color colorText, Color colorLetter)
        {
            foreColorLetter = colorLetter;
            ForeColor = colorText;
        }

        public async void Choose()
        {
            SetForeColors(Color.Black, Color.White);

            Bitmap[] frames = ResourceProcessing.FramesAppearance(CustomButton.ImageButton[ThemeButton.Orange], 6);

            foreach (var img in frames)
            {
                Image = img;
                await Task.Delay(MainForm.DeltaTime);
            }

            Image = CustomButton.ImageButton[ThemeButton.Orange];

            foreach (var img in frames)
                img.Dispose();
        }

        public async Task Blink()
        {
            SetForeColors(Color.White, Color.Black);

            int countFrames = 6;
            Bitmap[] frames;

            if (Image == null)
                frames = ResourceProcessing.FramesAppearance(CustomButton.ImageButton[ThemeButton.Green], countFrames);
            else
                frames = ResourceProcessing.FramesTransition((Bitmap)Image, CustomButton.ImageButton[ThemeButton.Green], countFrames);

            int n = frames.Length - 1;
            int n2 = 2 * n;
            int n5 = 5 * n;

            for (int i = 0; i <= n5; i++)
            {
                Image = frames[n - Math.Abs(i % n2 - n)];
                await Task.Delay(MainForm.DeltaTime);
            }

            Image = CustomButton.ImageButton[ThemeButton.Green];

            foreach (var img in frames)
                img.Dispose();
        }

        public async Task Clear(int countFrames)
        {
            if (Image == null)
            {
                await HideText(countFrames);
            }
            else
            {
                int[] alphas = Enumerable.Range(0, countFrames).Select(x => byte.MaxValue * x / (countFrames - 1)).Reverse().ToArray();
                Bitmap[] frames = ResourceProcessing.FramesDisappearance((Bitmap)Image, countFrames);

                for (int i = 0; i < countFrames; i++)
                {
                    alpha = alphas[i];
                    Image = frames[i];
                    await Task.Delay(MainForm.DeltaTime);
                }

                Reset();

                foreach (var img in frames)
                    img.Dispose();
            }
        }

        public void Lock()
        {
            Enabled = false;
            SetForeColors(Color.White, Color.Orange);
            Image = CustomButton.ImageButton[ThemeButton.Gray];
        }
    }
}
