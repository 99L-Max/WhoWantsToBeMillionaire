using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    partial class MainForm : Form, ISettable
    {
        private readonly Scene _scene;
        private readonly MenuMain _menuMain;
        private readonly StatisticsData _statisticsData;
        private readonly AchievementsData _achievementsData;
        private readonly AchievementShower _achievementShower;

        private bool _isShowScreenSaver = true;
        private ContextMenu _contextMenu;
        private SettingsData _settingsData;

        public MainForm()
        {
            InitializeComponent();

            Cursor = new Cursor(Resources.Cursor.GetHicon());
            BackgroundImage = new Bitmap(Resources.Background_Main, GameConst.ScreenSize);

            _scene = new Scene();
            _menuMain = new MenuMain();
            _settingsData = FileReader.GetSettingsDataOrDefault();
            _statisticsData = FileReader.GetStatisticsDataOrDefault();
            _achievementsData = FileReader.GetAchievementsDataOrDefault();
            _achievementShower = new AchievementShower(0.12f, 5, 1, 5, 250 / GameConst.DeltaTime, 5000, this);

            SetSettings(_settingsData);
            _settingsData.ApplyGlobal();
            _scene.Visible = false;

            _menuMain.ButtonClick += OnMainMenuClick;
            _scene.GameOver += GameOver;
            _scene.StatisticsChanged += UpdateStatistics;

            if (_achievementsData.HaveAllGranted == false)
                _scene.AchievementCompleted += GrantAchievement;

            Controls.Add(_scene);
            Controls.Add(_menuMain);

            _menuMain.SetCommands(MenuMain.GetCommands.Where(command => command != MainMenuCommands.Continue).ToArray());

            Music.Play(Resources.MainMenuTheme);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                base.CreateParams.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public void SetSettings(SettingsData data)
        {
            _isShowScreenSaver = Convert.ToBoolean(data.GetSettings(GameSettings.ShowScreensaver));
            _scene.SetSettings(data);
        }

        private async void OnContextMenuClick(ContextMenuCommands command)
        {
            switch (command)
            {
                default:
                    CloseContextMenu();
                    break;

                case ContextMenuCommands.StartGame:
                    if (_contextMenu is MenuMode menuMode)
                        _scene.Reset(menuMode.SelectedMode);
                    else
                        _scene.Reset(_scene.Mode);

                    _menuMain.Visible = false;
                    _menuMain.SetCommands(MenuMain.GetCommands.Where(cmd => cmd != MainMenuCommands.NewGame).ToArray());

                    Music.Stop();
                    CloseContextMenu();

                    if (_isShowScreenSaver)
                        await ShowScreenSaver(true);

                    _scene.Visible = true;
                    _scene.Start();
                    break;

                case ContextMenuCommands.ApplySettings:
                    if (_contextMenu is MenuSettings menuSettings)
                    {
                        _settingsData = new SettingsData(menuSettings.CopySettingsData);
                        SetSettings(_settingsData);
                        CloseContextMenu();
                    }
                    break;

                case ContextMenuCommands.Exit:
                    Close();
                    break;
            }
        }

        private async void GameOver(bool isRestart)
        {
            _scene.Visible = false;

            if (isRestart)
            {
                _scene.Reset(_scene.Mode);

                if (_isShowScreenSaver)
                    await ShowScreenSaver(false);

                _scene.Start(_scene.Visible = true);
            }
            else
            {
                _menuMain.SetCommands(MenuMain.GetCommands.Where(command => command != MainMenuCommands.Continue).ToArray());
                _menuMain.Visible = true;

                Music.Play(Resources.MainMenuTheme);
            }
        }

        private async Task ShowScreenSaver(bool isFullVersion)
        {
            using (var saver = new Screensaver())
            {
                Controls.Add(saver);
                Cursor.Hide();

                await saver.ShowSaver(isFullVersion);

                Controls.Remove(saver);
                Cursor.Show();
            }
        }

        private void CloseContextMenu()
        {
            _menuMain.Controls.Remove(_contextMenu);
            _menuMain.ButtonsVisible = true;

            if (_contextMenu is MenuSettings)
                _settingsData.ApplyGlobal();

            _contextMenu.ButtonClick -= OnContextMenuClick;
            _contextMenu.Dispose();
        }

        private void UpdateStatistics(StatisticsAttributes attribute, int value)
        {
            _statisticsData.Update(attribute, value);

            if (_statisticsData.GetAttribute(StatisticsAttributes.TotalPrize) >= 10000000)
                GrantAchievement(Achievements.Jubilee);
        }

        private void GrantAchievement(Achievements achievement)
        {
            if (_achievementsData.CheckGranted(achievement) == false)
            {
                _achievementsData.Grant(achievement);

                if (_achievementsData.HaveAllGranted)
                    _scene.AchievementCompleted -= GrantAchievement;

                _achievementShower.ShowAchievement(achievement);
            }
        }

        private void OnMainMenuClick(MainMenuCommands command)
        {
            switch (command)
            {
                case MainMenuCommands.Continue:
                    _menuMain.Visible = false;
                    _scene.Visible = true;
                    break;

                case MainMenuCommands.NewGame:
                    OpenContextMenu(new MenuMode(0.67f, 7, 9));
                    break;

                case MainMenuCommands.Statistics:
                    OpenContextMenu(new MenuStatistics(0.75f, 8, 9, _statisticsData.ToString()));
                    break;

                case MainMenuCommands.Achievements:
                    OpenContextMenu(new MenuAchievements(0.75f, 1, 1, _achievementsData.CopyAchievements));
                    break;

                case MainMenuCommands.Settings:
                    OpenContextMenu(new MenuSettings(0.67f, 4, 3, _settingsData));
                    break;

                case MainMenuCommands.Exit:
                    OpenContextMenu(new MenuExit(0.3f, 16, 9));
                    break;
            }
        }

        private void OpenContextMenu(ContextMenu menu)
        {
            _contextMenu = menu;
            _contextMenu.ButtonClick += OnContextMenuClick;

            _menuMain.ButtonsVisible = false;
            _menuMain.Controls.Add(_contextMenu);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_menuMain.Controls.Contains(_contextMenu))
                {
                    CloseContextMenu();
                }
                else if (_scene.IsMenuAvailable)
                {
                    _menuMain.Visible = !_menuMain.Visible;
                    _scene.Visible = !_scene.Visible;
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            GameDirectory.CreateSaveDirectory();

            _settingsData.Save();
            _statisticsData.Save();
            _achievementsData.Save();
        }
    }
}