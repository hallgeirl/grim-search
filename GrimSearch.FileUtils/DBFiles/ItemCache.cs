using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public class ItemCache
    {
        const string CurrentVersion = "1.1";
        ItemCacheContainer _cache = new ItemCacheContainer();

        public string CacheFilename { get; set; }

        private ItemCache()
        {
            CacheFilename = "ItemsCache.json";
        }

        public ItemRaw GetItem(string path)
        {
            if (_cache.Items.ContainsKey(path))
                return _cache.Items[path];

            return null;
        }

        public void LoadAllItems(string grimDawnDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback)
        {
            if (File.Exists(CacheFilename))
            {
                stateChangeCallback("Loading items from cache (" + CacheFilename + ")");
                LogHelper.GetLog().Debug("Found cache version: " + _cache.GrimDawnLastUpdated);
                _cache = JsonConvert.DeserializeObject<ItemCacheContainer>(File.ReadAllText(CacheFilename));
                LogHelper.GetLog().Debug("Items loaded from cache");
            }
        
            if (!skipVersionCheck)
            {
                string gdLastUpdated = GetGrimDawnLastUpdated(grimDawnDirectory);
                if (_cache.GrimDawnLastUpdated != gdLastUpdated || _cache.Version != CurrentVersion)
                {
                    LogHelper.GetLog().Debug("Either Grim Dawn has been updated, or the Grim Search cache format has been changed. Clearing cache.");
                    ClearCache();
                    _cache = null;
                }
            }

            if (!File.Exists(CacheFilename) || _cache == null)
            {
                stateChangeCallback("Loading items Grim Dawn database");

                _cache = new ItemCacheContainer();
                LogHelper.GetLog().Debug("Item cache not found - reading from " + grimDawnDirectory);
                ReadItemsFromFiles(grimDawnDirectory, keepExtractedFiles, stateChangeCallback);
                _cache.GrimDawnLastUpdated = GetGrimDawnLastUpdated(grimDawnDirectory);
                _cache.Version = CurrentVersion;
                File.WriteAllText(CacheFilename, JsonConvert.SerializeObject(_cache));
            }
        }

        public void ClearCache()
        {
            if (File.Exists(CacheFilename))
            {
                File.Delete(CacheFilename);
            }
        }

        static ItemCache _instance = null;
        static object _lock = new object();
        public static ItemCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ItemCache();
                        }
                    }
                }
                return _instance;
            }
        }


        private void ReadItemsFromFiles(string grimDawnDirectory, bool keepExtractedFiles, Action<string> stateChangeCallback)
        {
            string[] dbFiles = GetDBFilesWithFullPaths(grimDawnDirectory);

            int i = 0;
            foreach (var file in dbFiles)
            {
                i++;

                stateChangeCallback("Extracting DB file " + file + " (" + i + " of " + dbFiles.Length + ")");
                LogHelper.GetLog().Debug("Processing: " + file);

                var extractPath = Path.Combine(Path.GetTempPath(), "GDArchiveTempPath", Path.GetFileNameWithoutExtension(file) + "_" + Guid.NewGuid().ToString());

                ArzExtractor.Extract(file, grimDawnDirectory, extractPath);

                stateChangeCallback("Reading items (file " + i + " of " + dbFiles.Length + ")");
                PopulateAllItems(extractPath, stateChangeCallback);

                if (!keepExtractedFiles)
                    Directory.Delete(extractPath, true);

                MD5Store.Instance.SetHash(file);
            }
        }

        private static string[] GetDBFilesWithFullPaths(string grimDawnDirectory)
        {
            string[] dbFiles = {
                "database\\database.arz",
                "gdx1\\database\\GDX1.arz",
                "gdx2\\database\\GDX2.arz"
            };

            List<string> dbFilesThatExist = new List<string>();

            foreach (var file in dbFiles)
            {

                var fullFilePath = Path.Combine(grimDawnDirectory, file);
                if (!File.Exists(fullFilePath))
                    continue;

                dbFilesThatExist.Add(fullFilePath);
            }

            return dbFilesThatExist.ToArray();
        }

        private void PopulateAllItems(string path, Action<string> stateChangeCallback)
        {
            var itemsDirs = new string[] {
                Path.Combine(path, "records", "items"),
                Path.Combine(path, "records", "skills"),
                Path.Combine(path, "records", "storyelements"),
                Path.Combine(path, "records", "storyelementsgdx2"),
                Path.Combine(path, "records", "endlessdungeon", "items")
            };

            int i = 0;

            foreach (var itemsDir in itemsDirs)
            {
                if (!Directory.Exists(itemsDir))
                    continue;

                foreach (var f in Directory.EnumerateFiles(itemsDir, "*.dbr", SearchOption.AllDirectories))
                {
                    if (++i % 1000 == 0)
                        stateChangeCallback("Read item #" + i);

                    var relativePath = f.Replace(path, "").Trim('\\').Trim('/').Replace('\\', '/');
                    var item = new ItemRaw();
                    item.Read(f);

                    _cache.Items[relativePath] = item;
                }
            }

            stateChangeCallback("Read item " + i + " of " + i);
        }

        private string GetGrimDawnLastUpdated(string grimDawnDirectory)
        {
            var dbFiles = GetDBFilesWithFullPaths(grimDawnDirectory);

            DateTime maxDate = new DateTime(0);
            foreach (var dbFile in dbFiles)
            {
                var fileInfo = File.GetLastWriteTimeUtc(dbFile);
                if (fileInfo > maxDate)
                    maxDate = fileInfo;
            }

            return maxDate.ToString("yyyy-MM-dd");
        }
    }
}
