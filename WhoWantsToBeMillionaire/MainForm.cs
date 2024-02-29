using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    public partial class MainForm : Form
    {
        public const int DeltaTime = 40;
        public static readonly ReadOnlyRectangle RectScreen = new ReadOnlyRectangle(Screen.PrimaryScreen.Bounds);

        private readonly GameScene scene;
        private readonly MenuMain mainMenu;
        private readonly StatisticsData statisticsData;

        private ContextMenu contextMenu;

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
            mainMenu = new MenuMain();
            statisticsData = new StatisticsData(FileManager.PathLocalAppData);

            scene.Visible = false;

            mainMenu.ButtonClick += OnButtonMainMenuClick;
            scene.StatisticsChanged += UpdateStatistics;

            Controls.Add(scene);
            Controls.Add(mainMenu);

            mainMenu.SetCommands(new MainMenuCommand[] { MainMenuCommand.Start, MainMenuCommand.Achievements, MainMenuCommand.Statistics, MainMenuCommand.Settings, MainMenuCommand.Exit });
        }

        private void OnButtonMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    Close();
                    break;

                case MainMenuCommand.Start:
                    contextMenu = new MenuMode(RectScreen.Width / 3, RectScreen.Height * 2 / 3);
                    contextMenu.ButtonClick += OnButtonContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.ButtonsVisible = false;
                    break;

                case MainMenuCommand.Statistics:
                    contextMenu = new MenuStatistics(RectScreen.Width / 3, RectScreen.Height * 2 / 3, statisticsData);
                    contextMenu.ButtonClick += OnButtonContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.ButtonsVisible = false;
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
                    Settings.Default.Mode = (int)(contextMenu as MenuMode).SelectedMode;
                    Settings.Default.Save();
                    scene.Reset();

                    mainMenu.Visible = false;
                    CloseContextMenu();

                    //using (Screensaver saver = new Screensaver())
                    //{
                    //    Controls.Add(saver);
                    //
                    //    await saver.ShowSaver();
                    //
                    //    Controls.Remove(saver);
                    //}

                    scene.Visible = true;
                    scene.Start();
                    break;

                case ContextMenuCommand.ResetStatistics:
                    MessageBox.Show("DELETED");
                    break;
            }
        }

        private void UpdateStatistics(StatsAttribute attribute, int value)
        {
            statisticsData.Update(attribute, value);
        }

        private void CloseContextMenu()
        {
            if (contextMenu != null)
            {
                mainMenu.ButtonsVisible = true;
                mainMenu.Controls.Remove(contextMenu);
                contextMenu.ButtonClick -= OnButtonContextMenuClick;
                contextMenu.Dispose();
            }
        }
    }
}
