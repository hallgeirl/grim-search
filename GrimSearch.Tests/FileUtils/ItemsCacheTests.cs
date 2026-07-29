using System;
using System.Diagnostics;
using System.IO;
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
    }
}
