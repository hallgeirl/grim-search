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
                var itemsDir = Path.Combine(path, "records", "items");
                foreach (var f in Directory.EnumerateFiles(itemsDir, "*.dbr", SearchOption.AllDirectories))
                {
                    var relativePath = f.Replace(path, "").Trim('\\').Trim('/').Replace('\\', '/');
                    var item = new ItemRaw();
                    item.Read(f);

                    AllItems[relativePath] = item;
                }

                Directory.Delete(path, true);
            }
        }

    }
}
