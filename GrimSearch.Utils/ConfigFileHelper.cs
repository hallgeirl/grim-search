using System;
using System.IO;

namespace GrimSearch.Utils
{
    public static class ConfigFileHelper
    {
        public static string GetConfigFolder()
        {
            var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "grimsearch");
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            return configFolder;
        }

        public static string GetConfigFile(string filename)
        {
            return Path.Combine(GetConfigFolder(), filename);
        }
    }
}