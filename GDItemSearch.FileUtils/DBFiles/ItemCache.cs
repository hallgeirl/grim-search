using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils.DBFiles
{
    public class ItemCache
    {
        Dictionary<string, ItemRaw> AllItems = new Dictionary<string, ItemRaw>();

        string cacheFilename = "ItemsCache.json";

        private ItemCache()
        {
        }

        public ItemRaw GetItem(string path)
        {
            if (AllItems.ContainsKey(path))
                return AllItems[path];

            return null;
        }

        public void LoadAllItems()
        {
            if (File.Exists(cacheFilename))
                AllItems = JsonConvert.DeserializeObject<Dictionary<string, ItemRaw>>(File.ReadAllText(cacheFilename));
            else
            {
                ReadItemsFromFiles();
                File.WriteAllText(cacheFilename, JsonConvert.SerializeObject(AllItems));
            }

            
        }

        public void ClearCache()
        {
            if (File.Exists(cacheFilename))
            {
                File.Delete(cacheFilename);
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


        private void ReadItemsFromFiles()
        {
            string[] dbFiles = {
                "database\\database.arz",
                "gdx1\\database\\GDX1.arz"
            };

            foreach (var file in dbFiles)
            {
                var fullFilePath = Path.Combine(Settings.GrimDawnDirectory, file);
                var path = ArzExtractor.Extract(fullFilePath);
                PopulateAllItems(path);

                Directory.Delete(path, true);
            }
        }

        private void PopulateAllItems(string path)
        {
            var itemsDir = Path.Combine(path, "records", "items");
            foreach (var f in Directory.EnumerateFiles(itemsDir, "*.dbr", SearchOption.AllDirectories))
            {
                var relativePath = f.Replace(path, "").Trim('\\').Trim('/').Replace('\\', '/');
                var item = new ItemRaw();
                item.Read(f);

                AllItems[relativePath] = item;
            }
        }
    }
}
