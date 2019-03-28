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

        public void LoadAllItems(string grimDawnDirectory, Action<string> stateChangeCallback)
        {
            if (File.Exists(CacheFilename))
            {
                stateChangeCallback("Loading items from cache (" + CacheFilename + ")");
                LogHelper.GetLog().Debug("Found cache version: " + _cache.Version);
                _cache = JsonConvert.DeserializeObject<ItemCacheContainer>(File.ReadAllText(CacheFilename));
                LogHelper.GetLog().Debug("Items loaded from cache");
            }

            if (!File.Exists(CacheFilename) || _cache == null)
            {
                stateChangeCallback("Loading items Grim Dawn database");

                _cache = new ItemCacheContainer();
                LogHelper.GetLog().Debug("Item cache not found - reading from " + grimDawnDirectory);
                ReadItemsFromFiles(grimDawnDirectory, stateChangeCallback);
                _cache.Version = GetGrimDawnVersion(grimDawnDirectory);
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


        private void ReadItemsFromFiles(string grimDawnDirectory, Action<string> stateChangeCallback)
        {
            string[] dbFiles = {
                "database\\database.arz",
                "gdx1\\database\\GDX1.arz",
                "gdx2\\database\\GDX2.arz"
            };

            int i = 0;
            foreach (var file in dbFiles)
            {
                i++;
                var fullFilePath = Path.Combine(grimDawnDirectory, file);
                stateChangeCallback("Extracting DB file " + file + " (" + i + " of " + dbFiles.Length + ")");
                LogHelper.GetLog().Debug("Processing: " + fullFilePath);
                var path = ArzExtractor.Extract(fullFilePath, grimDawnDirectory);

                stateChangeCallback("Reading items (file " + i + " of " + dbFiles.Length + ")");
                PopulateAllItems(path, stateChangeCallback);

                Directory.Delete(path, true);
            }
        }

        private void PopulateAllItems(string path, Action<string> stateChangeCallback)
        {
            var itemsDirs = new string[] {
                Path.Combine(path, "records", "items"),
                Path.Combine(path, "records", "storyelements"),
                Path.Combine(path, "records", "storyelementsgdx2")
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

        private string GetGrimDawnVersion(string grimDawnDirectory)
        {
            var gdExe = Path.Combine(grimDawnDirectory, "Grim Dawn.exe");

            var fileInfo = File.GetLastWriteTimeUtc(gdExe);

            return fileInfo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
