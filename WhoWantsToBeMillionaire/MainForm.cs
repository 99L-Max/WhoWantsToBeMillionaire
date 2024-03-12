using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    partial class MainForm : Form, IGameSettings
    {
        public const int DeltaTime = 40;
        public static readonly ReadOnlyRectangle RectScreen = new ReadOnlyRectangle(Screen.PrimaryScreen.Bounds);

        private readonly Scene scene;
        private readonly MenuMain mainMenu;
        private readonly StatisticsData statisticsData;
        private readonly GameSettingsData settingsData;

        private ContextMenu contextMenu;
        private bool showScreenSaver = true;

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

            scene = new Scene();
            mainMenu = new MenuMain();
            statisticsData = new StatisticsData(FileManager.PathLocalAppData);
            settingsData = new GameSettingsData(FileManager.PathLocalAppData);

            scene.Visible = false;

            mainMenu.ButtonClick += OnMainMenuClick;
            scene.StatisticsChanged += UpdateStatistics;
            scene.SceneRestarting += RestartingScene;
            scene.ExitToMainMenu += ExitToMainMenu;

            Controls.Add(scene);
            Controls.Add(mainMenu);

            mainMenu.SetCommands(MainMenuCommand.NewGame, MainMenuCommand.Achievements, MainMenuCommand.Statistics, MainMenuCommand.Settings, MainMenuCommand.Exit);
        }

        private void OnMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    statisticsData.Save(FileManager.PathLocalAppData);
                    Close();
                    break;

                case MainMenuCommand.NewGame:
                    contextMenu = new MenuMode(RectScreen.Width / 3, RectScreen.Height * 2 / 3);
                    contextMenu.ButtonClick += OnContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.ButtonsVisible = false;
                    break;

                case MainMenuCommand.Settings:
                    contextMenu = new MenuSettings(RectScreen.Width / 3, RectScreen.Height * 2 / 3, settingsData);
                    contextMenu.ButtonClick += OnContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.ButtonsVisible = false;
                    break;

                case MainMenuCommand.Statistics:
                    contextMenu = new MenuStatistics(RectScreen.Width / 3, RectScreen.Height * 2 / 3, statisticsData.ToString());
                    contextMenu.ButtonClick += OnContextMenuClick;

                    mainMenu.Controls.Add(contextMenu);
                    mainMenu.ButtonsVisible = false;
                    break;

                case MainMenuCommand.Achievements:
                    MessageBox.Show("Achievements");
                    break;
            }
        }

        private async void OnContextMenuClick(ContextMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    CloseContextMenu();
                    break;

                case ContextMenuCommand.StartGame:
                    scene.Reset((contextMenu as MenuMode).SelectedMode);

                    mainMenu.Visible = false;
                    CloseContextMenu();

                    //await ShowScreenSaver(true);

                    scene.Visible = true;
                    scene.Start();
                    break;
            }
        }

        private async void RestartingScene()
        {
            scene.Visible = false;
            scene.Reset();

            await ShowScreenSaver(false);

            scene.Visible = true;
            scene.Restart();
        }

        private void ExitToMainMenu()
        {
            scene.Visible = false;
            mainMenu.Visible = true;
        }

        private async Task ShowScreenSaver(bool isFirst)
        {
            using (Screensaver saver = new Screensaver())
            {
                Controls.Add(saver);

                await saver.ShowSaver(isFirst);

                Controls.Remove(saver);
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
                contextMenu.ButtonClick -= OnContextMenuClick;
                contextMenu.Dispose();
            }
        }

        public void SetSettings(GameSettingsData data)
        {
            showScreenSaver = (bool)data.GetSettings(GameSettings.ShowScreensaver);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            statisticsData.Save(FileManager.PathLocalAppData);
            settingsData.Save(FileManager.PathLocalAppData);
        }
    }
}
