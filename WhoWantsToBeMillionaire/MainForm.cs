using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    public partial class MainForm : Form
    {
        private readonly GameScene scene;
        private readonly MainMenu menu;

        private readonly Bitmap background;

        public static readonly Rectangle RectScreen = Screen.PrimaryScreen.Bounds;

        public const int DeltaTime = 60;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                base.CreateParams.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            background = new Bitmap(ResourceProcessing.GetImage("Background_Main.png"), RectScreen.Size);

            scene = new GameScene();
            scene.Visible = false;

            menu = new MainMenu(new Size(RectScreen.Width / 3, RectScreen.Height / 2));
            menu.ButtonClick += OnButtonMenuClick;

            Controls.Add(scene);
            Controls.Add(menu);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(background, ClientRectangle);

            if (menu.Visible)
                using (Brush brush = new SolidBrush(Color.FromArgb(byte.MaxValue >> 1, Color.Black)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);

            base.OnPaint(e);
        }

        private async void OnButtonMenuClick(MenuCommand cmd)
        {
            switch (cmd)
            {
                case MenuCommand.Start:
                    menu.Visible = false;

                    //using (Screensaver saver = new Screensaver())
                    //{
                    //    Controls.Add(saver);
                    //    await saver.Show();
                    //    Controls.Remove(saver);
                    //}

                    scene.Visible = true;
                    scene.ShowRules();
                    break;

                default:
                    Close();
                    break;
            }
        }
    }
}
