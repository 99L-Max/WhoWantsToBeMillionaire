﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    partial class MainForm : Form, ISetSettings
    {
        private readonly Scene _scene;
        private readonly MenuMain _menuMain;
        private readonly StatisticsData _statisticsData;
        private readonly AchievementsData _achievementsData;
        private readonly AchievementShower _achievementShower;

        private bool _showScreenSaver = true;
        private ContextMenu _contextMenu;
        private GameSettingsData _settingsData;

        public MainForm()
        {
            InitializeComponent();

            Cursor = new Cursor(Resources.Cursor.GetHicon());
            BackgroundImage = new Bitmap(Resources.Background_Main, GameConst.ScreenSize);

            _scene = new Scene();
            _menuMain = new MenuMain();
            _settingsData = new GameSettingsData(FileManager.PathLocalAppData);
            _statisticsData = new StatisticsData(FileManager.PathLocalAppData);
            _achievementsData = new AchievementsData(FileManager.PathLocalAppData);
            _achievementShower = new AchievementShower(GameConst.ScreenSize.Width >> 2, GameConst.ScreenSize.Height >> 3, 5, 250 / GameConst.DeltaTime, 5000, this);

            SetSettings(_settingsData);
            _settingsData.ApplyGlobal();
            _scene.Visible = false;

            _menuMain.ButtonClick += OnMainMenuClick;
            _scene.GameOver += GameOver;
            _scene.StatisticsChanged += UpdateStatistics;

            if (!_achievementsData.AllGranted)
                _scene.AchievementCompleted += GrantAchievement;

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

        private void OnMainMenuClick(MainMenuCommand command)
        {
            switch (command)
            {
                case MainMenuCommand.Continue:
                    _menuMain.Visible = false;
                    _scene.Visible = true;
                    break;

                case MainMenuCommand.NewGame:
                    OpenContextMenu(new MenuMode(GameConst.ScreenSize.Width / 3, GameConst.ScreenSize.Height * 2 / 3));
                    break;

                case MainMenuCommand.Statistics:
                    OpenContextMenu(new MenuStatistics(GameConst.ScreenSize.Width / 3, GameConst.ScreenSize.Height * 2 / 3, _statisticsData.ToString()));
                    break;

                case MainMenuCommand.Achievements:
                    OpenContextMenu(new MenuAchievements(GameConst.ScreenSize.Width / 2, GameConst.ScreenSize.Height * 3 / 4, _achievementsData.Achievements));
                    break;

                case MainMenuCommand.Settings:
                    OpenContextMenu(new MenuSettings(GameConst.ScreenSize.Width / 2, GameConst.ScreenSize.Height * 2 / 3, _settingsData));
                    break;

                default:
                    OpenContextMenu(new MenuExit(GameConst.ScreenSize.Width / 3, GameConst.ScreenSize.Height / 3));
                    break;
            }
        }

        private async void OnContextMenuClick(ContextMenuCommand command)
        {
            switch (command)
            {
                default:
                    CloseContextMenu();
                    break;

                case ContextMenuCommand.StartGame:
                    if (_contextMenu is MenuMode menuMode)
                        _scene.Reset(menuMode.SelectedMode);
                    else
                        _scene.Reset(_scene.Mode);

                    _menuMain.Visible = false;
                    _menuMain.SetCommands(MenuMain.GetCommands.Where(x => x != MainMenuCommand.NewGame).ToArray());

                    CloseContextMenu();

                    if (_showScreenSaver)
                        await ShowScreenSaver(true);

                    _scene.Visible = true;
                    _scene.Start();
                    break;

                case ContextMenuCommand.ApplySettings:
                    if (_contextMenu is MenuSettings menuSettings)
                    {
                        _settingsData = new GameSettingsData(menuSettings.SettingsData);

                        SetSettings(_settingsData);
                        CloseContextMenu();
                    }
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
            using (var saver = new Screensaver())
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

        private void GrantAchievement(Achievement achievement)
        {
            if (!_achievementsData.CheckGranted(achievement))
            {
                _achievementsData.Grant(achievement);

                if (_achievementsData.AllGranted)
                    _scene.AchievementCompleted -= GrantAchievement;

                _achievementShower.ShowAchievement(achievement);
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