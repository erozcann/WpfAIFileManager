using System;
using System.IO;

namespace WpfAIFileManager
{
    public static class AppPaths
    {
        public static string BaseDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BASE");

        public static void EnsureBaseDirectoryExists()
        {
            if (!Directory.Exists(BaseDirectory))
                Directory.CreateDirectory(BaseDirectory);
        }
    }
}
