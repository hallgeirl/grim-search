using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils.DBFiles
{
    public class StringsCache
    {
        Dictionary<string, string> AllStrings = new Dictionary<string, string>();
        string cacheFilename = "TagsCache.json";
        bool initialized = false;

        private StringsCache()
        {
        }

        public string GetString(string tagName)
        {
            if (!initialized)
                throw new InvalidOperationException("StringsCache has not yet been initialized! Call LoadAllStrings FIRST before calling GetString.");

            if (AllStrings.ContainsKey(tagName))
                return AllStrings[tagName];

            return null;
        }

        public void LoadAllStrings()
        {
            if (File.Exists(cacheFilename))
                AllStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(cacheFilename));
            else
            {
                ReadTagsFromFiles();
                File.WriteAllText(cacheFilename, JsonConvert.SerializeObject(AllStrings));
            }

            initialized = true;
        }

        public void ClearCache()
        {
            if (File.Exists(cacheFilename))
            {
                File.Delete(cacheFilename);
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

        private void ReadTagsFromFiles()
        {
            string[] dbFiles = {
                "resources\\Text_EN.arc",
                "gdx1\\resources\\Text_EN.arc"
            };

            foreach (var file in dbFiles)
            {
                var fullFilePath = Path.Combine(Settings.GrimDawnDirectory, file);
                var path = ArzExtractor.Extract(fullFilePath);
                var tagsDir = Path.Combine(path, "text_en");
                foreach (var f in Directory.EnumerateFiles(tagsDir, "*.txt", SearchOption.AllDirectories))
                {
                    var tags = TagsReader.ReadAllTags(f);
                    foreach (var t in tags)
                    {
                        AllStrings[t.Key] = t.Value;
                    }
                }

                Directory.Delete(path, true);
            }
        }

    }
}
