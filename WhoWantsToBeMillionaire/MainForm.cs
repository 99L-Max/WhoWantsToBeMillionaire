using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    partial class MainForm : Form, IGameSettings
    {
        public const int DeltaTime = 40;
        public static readonly Rectangle ScreenRectangle = Screen.PrimaryScreen.Bounds;

        private readonly Scene _scene;
        private readonly MenuMain _mainMenu;
        private readonly StatisticsData _statisticsData;
        private readonly AchievementsData _achievementsData;

        private ContextMenu _contextMenu;
        private GameSettingsData _settingsData;
        private bool _showScreenSaver = true;

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

            BackgroundImage = new Bitmap(Resources.Background_Main, ScreenRectangle.Size);

            _scene = new Scene();
            _mainMenu = new MenuMain();
            _settingsData = new GameSettingsData(FileManager.PathLocalAppData);
            _statisticsData = new StatisticsData(FileManager.PathLocalAppData);
            _achievementsData = new AchievementsData(FileManager.PathLocalAppData);

            SetSettings(_settingsData);
            _settingsData.ApplyGlobal();
            _scene.Visible = false;

            _mainMenu.ButtonClick += OnMainMenuClick;
            _scene.GameOver += GameOver;
            _scene.StatisticsChanged += UpdateStatistics;
            _scene.AchievementСompleted += GrantAchievement;

            Controls.Add(_scene);
            Controls.Add(_mainMenu);

            _mainMenu.SetCommands(MainMenuCommand.NewGame, MainMenuCommand.Achievements, MainMenuCommand.Statistics, MainMenuCommand.Settings, MainMenuCommand.Exit);
        }

        private void OnMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                default:
                    OpenContextMenu(new MenuExit(ScreenRectangle.Width / 3, ScreenRectangle.Height / 3));
                    break;

                case MainMenuCommand.NewGame:
                    OpenContextMenu(new MenuMode(ScreenRectangle.Width / 3, ScreenRectangle.Height * 2 / 3));
                    break;

                case MainMenuCommand.Statistics:
                    OpenContextMenu(new MenuStatistics(ScreenRectangle.Width / 3, ScreenRectangle.Height * 2 / 3, _statisticsData.ToString()));
                    break;

                case MainMenuCommand.Achievements:
                    OpenContextMenu(new MenuAchievements(ScreenRectangle.Width / 2, ScreenRectangle.Height * 3 / 4, _achievementsData.Achievements));

                    break;

                case MainMenuCommand.Settings:
                    OpenContextMenu(new MenuSettings(ScreenRectangle.Width / 2, ScreenRectangle.Height * 2 / 3, _settingsData));
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
                    _scene.Reset((_contextMenu as MenuMode).SelectedMode);

                    _mainMenu.Visible = false;
                    CloseContextMenu();

                    if (_showScreenSaver)
                        await ShowScreenSaver(true);

                    _scene.Visible = true;
                    _scene.Start();
                    break;

                case ContextMenuCommand.ApplySettings:
                    _settingsData = new GameSettingsData((_contextMenu as MenuSettings).SettingsData);
                    SetSettings(_settingsData);
                    CloseContextMenu();
                    break;

                case ContextMenuCommand.Exit:
                    Close();
                    break;
            }
        }

        private void OpenContextMenu(ContextMenu menu)
        {
            _contextMenu = menu;
            _contextMenu.ButtonClick += OnContextMenuClick;

            _mainMenu.ButtonsVisible = false;
            _mainMenu.Controls.Add(_contextMenu);
        }

        private void CloseContextMenu()
        {
            _mainMenu.Controls.Remove(_contextMenu);
            _mainMenu.ButtonsVisible = true;
            _contextMenu.ButtonClick -= OnContextMenuClick;
            _contextMenu.Dispose();
        }

        private async void GameOver(bool isRestart)
        {
            _scene.Visible = false;
            _scene.Reset(_scene.Mode);

            if (isRestart)
            {
                if (_showScreenSaver)
                    await ShowScreenSaver(false);

                _scene.Start(_scene.Visible = true);
            }
            else
            {
                _mainMenu.Visible = true;
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

        private void UpdateStatistics(StatsAttribute attribute, int value)
        { 
            _statisticsData.Update(attribute, value);

            if (_statisticsData.GetAttribute(StatsAttribute.TotalPrize) >= 10000000)
                GrantAchievement(Achievement.Jubilee);
        }

        private async void GrantAchievement(Achievement achievement)
        {
            if (!_achievementsData.CheckGranted(achievement))
            {
                _achievementsData.Grant(achievement);

                int count = Controls.OfType<BoxAchievement>().Count();

                using (BoxAchievement box = new BoxAchievement(achievement, 280, 70))
                {
                    box.Y = -box.Height;
                    Controls.Add(box);

                    box.BringToFront();

                    Sound.Play(Resources.Achievement, false);

                    await box.MoveY(count * box.Height, 250 / DeltaTime);
                    await Task.Delay(5000);
                    await box.MoveX(-box.Width, 250 / DeltaTime);

                    Controls.Remove(box);
                }
            }
        }

        public void SetSettings(GameSettingsData data)
        {
            _showScreenSaver = Convert.ToBoolean(data.GetSettings(GameSettings.ShowScreensaver));
            _scene.SetSettings(data);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                if (_mainMenu.Controls.Contains(_contextMenu))
                {
                    CloseContextMenu();
                }
                else if (_scene.MenuAllowed)
                {
                    _mainMenu.Visible = !_mainMenu.Visible;
                    _scene.Visible = !_scene.Visible;
                }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            FileManager.CreateSaveDirectory();
            _settingsData.Save(FileManager.PathLocalAppData);
            _statisticsData.Save(FileManager.PathLocalAppData);
            //_achievementsData.Save(FileManager.PathLocalAppData);
        }
    }
}
