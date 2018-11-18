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
            ItemCache.Instance.CacheFilename = "Resources\\ItemsCache.json";

            ItemCache.Instance.LoadAllItems(null, (msg  ) => { });

            var item = ItemCache.Instance.GetItem("records/items/lootsets/itemset_d017.dbr");
            Assert.AreEqual("records/skills/itemskills/legendary/item_ultoswrath.dbr", item.StringParametersRaw["itemSkillName"]);
        }
    }
}
