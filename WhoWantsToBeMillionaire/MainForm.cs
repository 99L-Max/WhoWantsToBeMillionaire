using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    public partial class MainForm : Form
    {
        private readonly GameScene scene;
        private readonly MainMenu mainMenu;

        private ContextMenu contextMenu;

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
            BackgroundImageLayout = ImageLayout.Stretch;

            scene = new GameScene();
            mainMenu = new MainMenu();

            scene.Visible = false;

            mainMenu.ButtonClick += OnButtonMainMenuClick;

            Controls.Add(scene);
            Controls.Add(mainMenu);

            mainMenu.SetCommands(new MainMenuCommand[] { MainMenuCommand.Start, MainMenuCommand.Achievements, MainMenuCommand.Settings, MainMenuCommand.Exit });
        }

        private void OnButtonMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    Close();
                    break;

                case MainMenuCommand.Start:
                    contextMenu = new ModeMenu(new Size(RectScreen.Width / 3, RectScreen.Height * 2 / 3));
                    contextMenu.ButtonClick += OnButtonContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.TableVisible = false;
                    break;
            }
        }

        private async void OnButtonContextMenuClick(ContextMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    CloseContextMenu();
                    break;

                case ContextMenuCommand.StartGame:
                    Properties.Settings.Default.Mode = (int)(contextMenu as ModeMenu).SelectedMode;
                    Properties.Settings.Default.Save();

                    mainMenu.Visible = false;
                    CloseContextMenu();

                    scene.Reset();

                    //using (Screensaver saver = new Screensaver())
                    //{
                    //    Controls.Add(saver);
                    //    await saver.Show();
                    //    Controls.Remove(saver);
                    //}

                    scene.Visible = true;
                    scene.Start();
                    break;
            }
        }

        private void CloseContextMenu()
        {
            if (contextMenu != null)
            {
                mainMenu.TableVisible = true;
                mainMenu.Controls.Remove(contextMenu);
                contextMenu.ButtonClick -= OnButtonContextMenuClick;
                contextMenu.Dispose();
            }
        }
    }
}
