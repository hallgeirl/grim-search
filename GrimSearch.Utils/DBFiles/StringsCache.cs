using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public class StringsCache
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        Dictionary<string, string> AllStrings = new Dictionary<string, string>();
        public string CacheFilename { get; set; }
        public bool IsDirty { get; set; } = true;
        bool initialized = false;

        private StringsCache()
        {
            CacheFilename = ConfigFileHelper.GetConfigFile("TagsCache.json");
        }

        public string GetString(string tagName)
        {
            if (!initialized)
                throw new InvalidOperationException("StringsCache has not yet been initialized! Call LoadAllStrings FIRST before calling GetString.");

            if (AllStrings.ContainsKey(tagName))
                return AllStrings[tagName];

            return null;
        }

        public void LoadAllStrings(string grimDawnDirectory)
        {
            if (File.Exists(CacheFilename))
                AllStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(CacheFilename));
            else
            {
                ReadTagsFromFiles(grimDawnDirectory);
                File.WriteAllText(CacheFilename, JsonConvert.SerializeObject(AllStrings));
            }

            initialized = true;
            IsDirty = false;
        }

        public void ClearCache()
        {
            IsDirty = true;
            if (File.Exists(CacheFilename))
            {
                File.Delete(CacheFilename);
            }
        }

        static StringsCache _instance = null;
        static object _lock = new object();
        public static StringsCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new StringsCache();
                        }
                    }
                }
                return _instance;
            }
        }

        private string GetTagFileCopy(string tagFilePath)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var targetFile = Path.Combine(tempDir, Path.GetFileName(tagFilePath));
            _logger.Info("Copying " + tagFilePath + " to " + targetFile);
            Directory.CreateDirectory(tempDir);
            File.Copy(tagFilePath, targetFile);


            return targetFile;
        }
        private void ReadTagsFromFiles(string grimDawnDirectory)
        {
            string[] dbFiles = {
                "resources/Text_EN.arc",
                "gdx1/resources/Text_EN.arc",
                "gdx2/resources/Text_EN.arc"
            };

            foreach (var file in dbFiles)
            {
                var fullFilePath = Path.Combine(grimDawnDirectory, file);

                if (!File.Exists(fullFilePath))
                    continue;

                var tempArcFile = GetTagFileCopy(fullFilePath);
                string extractedPath = Path.Combine(Path.GetTempPath(), "GDArchiveTempPath", Path.GetFileNameWithoutExtension(file) + "_" + Guid.NewGuid().ToString());

                try
                {
                    ArzExtractor.ExtractArc(tempArcFile, extractedPath);
                    var tagsDir = extractedPath;
                    foreach (var f in Directory.EnumerateFiles(tagsDir, "*.txt", SearchOption.AllDirectories))
                    {
                        var tags = TagsReader.ReadAllTags(f);
                        foreach (var t in tags)
                        {
                            AllStrings[t.Key] = t.Value;
                        }
                    }
                    MD5Store.Instance.SetHash(fullFilePath);
                }
                finally
                {

                    if (extractedPath != null && Directory.Exists(extractedPath))
                        Directory.Delete(extractedPath, true);

                    var tempArcDir = Path.GetDirectoryName(tempArcFile);
                    if (Directory.Exists(tempArcDir))
                        Directory.Delete(tempArcDir, true);
                }


            }
        }

    }
}
