using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        private ContextMenu contextMenu;
        private GameSettingsData settingsData;
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

            BackgroundImage = new Bitmap(ResourceManager.GetImage("Background_Main.png"), RectScreen.Size);

            scene = new Scene();
            mainMenu = new MenuMain();
            statisticsData = new StatisticsData(FileManager.PathLocalAppData);
            settingsData = new GameSettingsData(FileManager.PathLocalAppData);

            SetSettings(settingsData);
            settingsData.ApplyGlobal();
            scene.Visible = false;

            mainMenu.ButtonClick += OnMainMenuClick;
            scene.StatisticsChanged += UpdateStatistics;
            scene.GameOver += GameOver;

            Controls.Add(scene);
            Controls.Add(mainMenu);

            mainMenu.SetCommands(MainMenuCommand.NewGame, MainMenuCommand.Achievements, MainMenuCommand.Statistics, MainMenuCommand.Settings, MainMenuCommand.Exit);
        }

        private void OnMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    Close();
                    break;

                case MainMenuCommand.NewGame:
                    OpenContextMenu(new MenuMode(RectScreen.Width / 3, RectScreen.Height * 2 / 3));
                    break;

                case MainMenuCommand.Statistics:
                    OpenContextMenu(new MenuStatistics(RectScreen.Width / 3, RectScreen.Height * 2 / 3, statisticsData.ToString()));
                    break;

                case MainMenuCommand.Achievements:
                    MessageBox.Show("Achievements");
                    break;

                case MainMenuCommand.Settings:
                    OpenContextMenu(new MenuSettings(RectScreen.Width / 2, RectScreen.Height * 2 / 3, settingsData));
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

                    if (showScreenSaver)
                        await ShowScreenSaver(true);

                    scene.Visible = true;
                    scene.Start();
                    break;

                case ContextMenuCommand.ApplySettings:
                    settingsData = new GameSettingsData((contextMenu as MenuSettings).SettingsData);
                    SetSettings(settingsData);
                    CloseContextMenu();
                    break;
            }
        }

        private void OpenContextMenu(ContextMenu menu)
        {
            contextMenu = menu;
            contextMenu.ButtonClick += OnContextMenuClick;

            mainMenu.Controls.Add(contextMenu);
            mainMenu.ButtonsVisible = false;
        }

        private void CloseContextMenu()
        {
            mainMenu.Controls.Remove(contextMenu);
            mainMenu.ButtonsVisible = true;
            contextMenu.ButtonClick -= OnContextMenuClick;
            contextMenu.Dispose();
        }

        private async void GameOver(bool isRestart)
        {
            if (isRestart)
            {
                scene.Visible = false;
                scene.Reset(scene.Mode);

                if (showScreenSaver)
                    await ShowScreenSaver(false);

                scene.Visible = true;
                scene.Restart();
            }
            else
            {
                scene.Visible = false;
                mainMenu.Visible = true;
            }
        }

        private async Task ShowScreenSaver(bool isFullVersion)
        {
            using (Screensaver saver = new Screensaver())
            {
                Controls.Add(saver);

                await saver.ShowSaver(isFullVersion);

                Controls.Remove(saver);
            }
        }

        private void UpdateStatistics(StatsAttribute attribute, int value) =>
            statisticsData.Update(attribute, value);

        public void SetSettings(GameSettingsData data)
        {
            showScreenSaver = Convert.ToBoolean(data.GetSettings(GameSettings.ShowScreensaver));
            scene.SetSettings(data);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (mainMenu.Controls.Contains(contextMenu))
                { 
                    CloseContextMenu();
                }
                else if (scene.MenuAllowed)
                {
                    mainMenu.Visible = !mainMenu.Visible;
                    scene.Visible = !scene.Visible;
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            FileManager.CreateSaveDirectory();
            statisticsData.Save(FileManager.PathLocalAppData);
            settingsData.Save(FileManager.PathLocalAppData);
        }
    }
}
