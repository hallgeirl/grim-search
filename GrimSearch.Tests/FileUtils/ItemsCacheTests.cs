using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrimSearch.Utils.DBFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class ItemsCacheTests
    {
        [TestMethod]
        public void TestLoadAllItemsFromCache()
        {
            ItemCache.Instance.CacheFilename = "Resources/ItemsCache.json";

            ItemCache.Instance.LoadAllItems(null, false, true, (msg) => { });

            var item = ItemCache.Instance.GetItem("records/items/lootsets/itemset_d017.dbr");
            Assert.AreEqual("records/skills/itemskills/legendary/item_ultoswrath.dbr", item.StringParametersRaw["itemSkillName"]);
        }

        [TestMethod]
        public void GetFullItemNameIgnoresAffixMissingFromCache()
        {
            ItemCache.Instance.CacheFilename = "Resources/ItemsCache.json";
            ItemCache.Instance.LoadAllItems(null, false, true, (msg) => { });

            var item = new GrimSearch.Utils.CharacterFiles.Item
            {
                prefixName = "records/items/lootaffixes/missing_prefix.dbr",
                suffixName = "records/items/lootaffixes/missing_suffix.dbr"
            };
            var itemDef = new ItemRaw();
            itemDef.StringParametersRaw["Class"] = "WeaponMelee_Sword";
            itemDef.StringParametersRaw["FileDescription"] = "Test Sword";

            Assert.AreEqual("Test Sword", ItemHelper.GetFullItemName(item, itemDef));
        }

        [TestMethod]
        public void ConcurrentCacheLoadsAreSerialized()
        {
            var cache = ItemCache.Instance;
            var originalCacheFilename = cache.CacheFilename;
            var tempCacheFilename = Path.GetTempFileName();
            File.WriteAllText(tempCacheFilename, "{\"Items\":{},\"Version\":\"1.2\"}");

            using var firstLoadEntered = new ManualResetEventSlim();
            using var releaseFirstLoad = new ManualResetEventSlim();
            using var secondLoadStarted = new ManualResetEventSlim();
            using var secondLoadEntered = new ManualResetEventSlim();

            try
            {
                cache.CacheFilename = tempCacheFilename;
                var firstLoad = Task.Run(() => cache.LoadAllItems(null, false, true, _ =>
                {
                    firstLoadEntered.Set();
                    releaseFirstLoad.Wait();
                }));

                Assert.IsTrue(firstLoadEntered.Wait(1000));

                var secondLoad = Task.Run(() =>
                {
                    secondLoadStarted.Set();
                    cache.LoadAllItems(null, false, true, _ => secondLoadEntered.Set());
                });
                Assert.IsTrue(secondLoadStarted.Wait(1000));
                Assert.IsFalse(secondLoadEntered.Wait(100));

                releaseFirstLoad.Set();
                Task.WaitAll(firstLoad, secondLoad);
                Assert.IsTrue(secondLoadEntered.IsSet);
            }
            finally
            {
                releaseFirstLoad.Set();
                cache.CacheFilename = originalCacheFilename;
                File.Delete(tempCacheFilename);
                if (File.Exists(originalCacheFilename))
                    cache.LoadAllItems(null, false, true, _ => { });
                else
                    cache.IsDirty = true;
            }
        }
    }
}
