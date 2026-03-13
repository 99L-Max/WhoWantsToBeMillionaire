using System;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    static class GameDirectory
    {
        static GameDirectory()
        {
            SavingPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{typeof(Program).Namespace}";
        }

        public static string SavingPath { get; }
        public static string SettingsFilePath => $@"{SavingPath}\Settings.json";
        public static string StatisticsFilePath => $@"{SavingPath}\Statistics.json";
        public static string AchievementsFilePath => $@"{SavingPath}\Achievements.json";

        public static void CreateSaveDirectory()
        {
            Directory.CreateDirectory(SavingPath);
        }
    }
}
