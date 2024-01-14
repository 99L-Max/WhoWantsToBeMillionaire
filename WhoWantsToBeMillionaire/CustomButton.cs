using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    enum ThemesButton
    {
        Blue,
        Orange,
        Green,
        Gray
    }

    enum ButtonCommand
    {
        Start,
        Restart,
        Continue,
        Statistics,
        Achievements,
        Settings,
        Exit
    }

    class CustomButton : PictureBox
    {
        private ButtonCommand command;

        public static readonly ReadOnlyDictionary<ThemesButton, Bitmap> ImageButton;
        public static readonly ReadOnlyDictionary<ButtonCommand, string> TextButton;

        public ButtonCommand Command
        {
            set
            {
                command = value;
                Text = TextButton[value];
            }
            get => command;
        }

        public new bool Enabled
        {
            set
            {
                base.Enabled = value;

                BackgroundImage = ImageButton[value ? ThemesButton.Blue : ThemesButton.Gray];
                ForeColor = value ? Color.White : Color.Black;
            }
            get => base.Enabled;
        }

        static CustomButton()
        {
            var img = new Dictionary<ThemesButton, Bitmap>();

            foreach (var key in Enum.GetValues(typeof(ThemesButton)).Cast<ThemesButton>())
                img.Add(key, new Bitmap(ResourceProcessing.GetImage($"Answer_{key}.png")));

            var cmd = new Dictionary<ButtonCommand, string>()
            {
                { ButtonCommand.Start, "Новая игра" },
                { ButtonCommand.Restart, "Новая игра" },
                { ButtonCommand.Continue, "Продолжить игру" },
                { ButtonCommand.Statistics, "Статистика" },
                { ButtonCommand.Achievements, "Достижения" },
                { ButtonCommand.Settings, "Настройки" },
                { ButtonCommand.Exit, "Выход" }
            };

            ImageButton = new ReadOnlyDictionary<ThemesButton, Bitmap>(img);
            TextButton = new ReadOnlyDictionary<ButtonCommand, string>(cmd);
        }

        public CustomButton(float sizeFont)
        {
            Font = new Font("", sizeFont, FontStyle.Bold);
            Dock = DockStyle.Fill;

            SetDefaultSettings();
        }

        public CustomButton(Size size)
        {
            Size = size;
            Font = new Font("", 0.35f * size.Height, FontStyle.Bold);

            SetDefaultSettings();
        }

        private void SetDefaultSettings()
        {
            BackColor = Color.Transparent;
            BackgroundImageLayout = ImageLayout.Zoom;
            OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled)
            {
                BackgroundImage = ImageButton[ThemesButton.Orange];
                ForeColor = Color.Black;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Enabled)
            {
                BackgroundImage = ImageButton[ThemesButton.Blue];
                ForeColor = Color.White;
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (Enabled)
                BackgroundImage = ImageButton[ThemesButton.Green];
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            OnMouseEnter(mevent);
        }
    }
}
