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

        public void LoadAllItems(string grimDawnDirectory)
        {
            if (File.Exists(CacheFilename))
            {
                LogHelper.GetLog().Debug("Found cache version: " + _cache.Version);
                _cache = JsonConvert.DeserializeObject<ItemCacheContainer>(File.ReadAllText(CacheFilename));
                LogHelper.GetLog().Debug("Items loaded from cache");
            }

            if (!File.Exists(CacheFilename) || _cache == null)
            {
                _cache = new ItemCacheContainer();
                LogHelper.GetLog().Debug("Item cache not found - reading from " + grimDawnDirectory);
                ReadItemsFromFiles(grimDawnDirectory);
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


        private void ReadItemsFromFiles(string grimDawnDirectory)
        {
            string[] dbFiles = {
                "database\\database.arz",
                "gdx1\\database\\GDX1.arz"
            };

            foreach (var file in dbFiles)
            {
                var fullFilePath = Path.Combine(grimDawnDirectory, file);
                LogHelper.GetLog().Debug("Processing: " + fullFilePath);
                var path = ArzExtractor.Extract(fullFilePath, grimDawnDirectory);
                PopulateAllItems(path);

                Directory.Delete(path, true);
            }
        }

        private void PopulateAllItems(string path)
        {
            var itemsDirs = new string[] {
                Path.Combine(path, "records", "items"),
                Path.Combine(path, "records", "storyelements")
            };

            foreach (var itemsDir in itemsDirs)
            {
                foreach (var f in Directory.EnumerateFiles(itemsDir, "*.dbr", SearchOption.AllDirectories))
                {
                    var relativePath = f.Replace(path, "").Trim('\\').Trim('/').Replace('\\', '/');
                    var item = new ItemRaw();
                    item.Read(f);

                    _cache.Items[relativePath] = item;
                }
            }
        }

        private string GetGrimDawnVersion(string grimDawnDirectory)
        {
            var gdExe = Path.Combine(grimDawnDirectory, "Grim Dawn.exe");

            var fileInfo = File.GetLastWriteTimeUtc(gdExe);

            return fileInfo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
