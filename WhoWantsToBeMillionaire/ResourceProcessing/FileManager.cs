using System;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    static class FileManager
    {
        public static readonly string PathLocalAppData;

        static FileManager() =>
            PathLocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $@"\{typeof(MainForm).Namespace}";

        public static void CreateSaveDirectory() =>
            Directory.CreateDirectory(PathLocalAppData);
    }
}
