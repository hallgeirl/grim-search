using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public class StringsCache
    {
        Dictionary<string, string> AllStrings = new Dictionary<string, string>();
        public string CacheFilename { get; set; }
        bool initialized = false;

        private StringsCache()
        {
            CacheFilename = "TagsCache.json"; 
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
        }

        public void ClearCache()
        {
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

        private void ReadTagsFromFiles(string grimDawnDirectory)
        {
            string[] dbFiles = {
                "resources\\Text_EN.arc",
                "gdx1\\resources\\Text_EN.arc"
            };

            foreach (var file in dbFiles)
            {
                var fullFilePath = Path.Combine(grimDawnDirectory, file);
                var path = ArzExtractor.Extract(fullFilePath, grimDawnDirectory);
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
