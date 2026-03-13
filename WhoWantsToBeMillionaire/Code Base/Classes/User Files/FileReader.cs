using Newtonsoft.Json;
using System;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    static class FileReader
    {
        public static AchievementsData GetAchievementsDataOrDefault()
        {
            return GetObjectFromFile<AchievementsData>(GameDirectory.AchievementsFilePath);
        }

        public static StatisticsData GetStatisticsDataOrDefault()
        {
            return GetObjectFromFile<StatisticsData>(GameDirectory.StatisticsFilePath);
        }

        public static SettingsData GetSettingsDataOrDefault()
        {
            return GetObjectFromFile<SettingsData>(GameDirectory.SettingsFilePath);
        }

        private static T GetObjectFromFile<T>(string path) where T : new()
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}
