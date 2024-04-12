using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    partial class MainForm : Form, ISetSettings
    {
        public const int DeltaTime = 40;
        public static readonly Size ScreenSize = Screen.PrimaryScreen.Bounds.Size;

        private readonly Scene _scene;
        private readonly MenuMain _menuMain;
        private readonly StatisticsData _statisticsData;
        private readonly AchievementsData _achievementsData;

        private bool _showScreenSaver = true;
        private ContextMenu _contextMenu;
        private GameSettingsData _settingsData;

        public MainForm()
        {
            InitializeComponent();

            Cursor = new Cursor(Resources.Cursor.GetHicon());
            BackgroundImage = new Bitmap(Resources.Background_Main, ScreenSize);

            _scene = new Scene();
            _menuMain = new MenuMain();
            _settingsData = new GameSettingsData(FileManager.PathLocalAppData);
            _statisticsData = new StatisticsData(FileManager.PathLocalAppData);
            _achievementsData = new AchievementsData(FileManager.PathLocalAppData);

            SetSettings(_settingsData);
            _settingsData.ApplyGlobal();
            _scene.Visible = false;

            _menuMain.ButtonClick += OnMainMenuClick;
            _scene.GameOver += GameOver;
            _scene.StatisticsChanged += UpdateStatistics;

            if (!_achievementsData.AllGranted)
                _scene.AchievementСompleted += GrantAchievement;

            Controls.Add(_scene);
            Controls.Add(_menuMain);

            _menuMain.SetCommands(MenuMain.GetCommands.Where(x => x != MainMenuCommand.Continue).ToArray());
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

        private void OnMainMenuClick(MainMenuCommand cmd)
        {
            switch (cmd)
            {
                case MainMenuCommand.Continue:
                    _menuMain.Visible = false;
                    _scene.Visible = true;
                    break;

                case MainMenuCommand.NewGame:
                    OpenContextMenu(new MenuMode(ScreenSize.Width / 3, ScreenSize.Height * 2 / 3));
                    break;

                case MainMenuCommand.Statistics:
                    OpenContextMenu(new MenuStatistics(ScreenSize.Width / 3, ScreenSize.Height * 2 / 3, _statisticsData.ToString()));
                    break;

                case MainMenuCommand.Achievements:
                    OpenContextMenu(new MenuAchievements(ScreenSize.Width / 2, ScreenSize.Height * 3 / 4, _achievementsData.Achievements));

                    break;

                case MainMenuCommand.Settings:
                    OpenContextMenu(new MenuSettings(ScreenSize.Width / 2, ScreenSize.Height * 2 / 3, _settingsData));
                    break;

                default:
                    OpenContextMenu(new MenuExit(ScreenSize.Width / 3, ScreenSize.Height / 3));
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

                    _menuMain.Visible = false;
                    _menuMain.SetCommands(MenuMain.GetCommands.Where(x => x != MainMenuCommand.NewGame).ToArray());

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

            _menuMain.ButtonsVisible = false;
            _menuMain.Controls.Add(_contextMenu);
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

        private async void GameOver(bool isRestart)
        {
            _scene.Visible = false;

            if (isRestart)
            {
                _scene.Reset(_scene.Mode);

                if (_showScreenSaver)
                    await ShowScreenSaver(false);

                _scene.Start(_scene.Visible = true);
            }
            else
            {
                _menuMain.SetCommands(MenuMain.GetCommands.Where(x => x != MainMenuCommand.Continue).ToArray());
                _menuMain.Visible = true;
            }
        }

        private async Task ShowScreenSaver(bool isFullVersion)
        {
            using (Screensaver saver = new Screensaver())
            {
                Controls.Add(saver);
                Cursor.Hide();

                await saver.ShowSaver(isFullVersion);

                Controls.Remove(saver);
                Cursor.Show();
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

                if (_achievementsData.AllGranted)
                    _scene.AchievementСompleted -= GrantAchievement;

                var countBoxes = Controls.OfType<BoxAchievement>().Count();

                using (BoxAchievement box = new BoxAchievement(achievement, 280, 70))
                {
                    box.Y = -box.Height;

                    Controls.Add(box);

                    box.BringToFront();

                    Sound.Play(Resources.Achievement, false);

                    await box.ShowAchievement(countBoxes * box.Height, 250 / DeltaTime, 5000);

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
                if (_menuMain.Controls.Contains(_contextMenu))
                {
                    CloseContextMenu();
                }
                else if (_scene.MenuAllowed)
                {
                    _menuMain.Visible = !_menuMain.Visible;
                    _scene.Visible = !_scene.Visible;
                }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            FileManager.CreateSaveDirectory();
            _settingsData.Save(FileManager.PathLocalAppData);
            _statisticsData.Save(FileManager.PathLocalAppData);
            _achievementsData.Save(FileManager.PathLocalAppData);
        }
    }
}