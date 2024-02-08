using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    public partial class MainForm : Form
    {
        private readonly GameScene scene;
        private readonly Menu menu;

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

            BackgroundImage = new Bitmap(ResourceProcessing.GetImage("Background_Main.png"), RectScreen.Size);

            scene = new GameScene();
            menu = new Menu(new Size(RectScreen.Width / 3, RectScreen.Height / 2));

            scene.Visible = false;

            menu.SetCommands(new MenuCommand[] { MenuCommand.Continue, MenuCommand.Start, MenuCommand.Achievements, MenuCommand.Settings, MenuCommand.Exit });
            menu.ButtonClick += OnButtonMenuClick;

            Controls.Add(scene);
            Controls.Add(menu);
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
