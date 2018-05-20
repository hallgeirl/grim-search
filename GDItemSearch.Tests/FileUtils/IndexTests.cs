using GDItemSearch.Utils;
using GDItemSearch.Utils.DBFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Tests.FileUtils
{
    [TestClass]
    public class IndexTests
    {
        [TestInitialize]
        public void Initialize()
        {
            StringsCache.Instance.CacheFilename = "Resources\\TagsCache.json";
            ItemCache.Instance.CacheFilename = "Resources\\ItemsCache.json";
        }

        [TestMethod]
        public void TestBuildIndex()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            Assert.IsTrue(summary.ItemRarities.Contains("Legendary"));
            Assert.IsTrue(summary.ItemRarities.Contains("Epic"));
            Assert.IsTrue(summary.ItemRarities.Contains("Rare"));

            Assert.IsTrue(summary.ItemTypes.Contains("WeaponMelee_Axe2h"));
        }

        [TestMethod]
        public void TestBuildIndexRepeated()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");
            var entries = summary.Entries;
            summary = index.Build(null, "Resources\\Saves");

            Assert.AreEqual(entries, summary.Entries);
        }


        [TestMethod]
        public void TestFindOnName()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Mythical Ultos' Stormseeker") > 0);
        }

        [TestMethod]
        public void TestFindOnCharacterName()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Owner == "Thorine") > 0);
        }


        [TestMethod]
        public void TestLevelRangeLimiterMinMaxLevelBaseline()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 0, MaxLevel = 100, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.LevelRequirement < 80) > 0);
            Assert.IsTrue(results.Count(x => x.LevelRequirement > 90) > 0);
        }

        [TestMethod]
        public void TestLevelRangeLimiterMinMaxLevel()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 80, MaxLevel = 90, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.LevelRequirement < 80) == 0);
            Assert.IsTrue(results.Count(x => x.LevelRequirement > 90) == 0);
        }

        [TestMethod]
        public void TestQualityLimiter()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");
            
            var results = index.Find("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", ItemQualities = new string[] { "Legendary", "Rare" }, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Epic") == 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Common") == 0);
        }

        [TestMethod]
        public void TestQualityLimiterBaseline()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Epic") > 0);
        }

        [TestMethod]
        public void TestFindDuplicates()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.FindDuplicates("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Find duplicates", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Mythical Signet of the Runefather") > 0);
        }

        [TestMethod]
        public void TestFindUnequippedWhenIsEquippedIsChecked()
        {
            var index = new Index();
            var summary = index.Build(null, "Resources\\Saves");

            var results = index.Find("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Fist of Ultos") > 0);
        }
    }
}
