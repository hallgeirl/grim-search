using GrimSearch.Utils;
using GrimSearch.Utils.DBFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Index = GrimSearch.Utils.Index;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class LuceneIndexTests
    {
        [TestInitialize]
        public void Initialize()
        {
            StringsCache.Instance.CacheFilename = "Resources/TagsCache.json";
            ItemCache.Instance.CacheFilename = "Resources/ItemsCache.json";
        }

        [TestMethod]
        public async Task TestBuildIndex()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            Assert.IsTrue(summary.ItemRarities.Contains("Legendary"));
            Assert.IsTrue(summary.ItemRarities.Contains("Epic"));
            Assert.IsTrue(summary.ItemRarities.Contains("Rare"));

            Assert.IsTrue(summary.ItemTypes.Contains("WeaponMelee_Axe2h"));
        }

        [TestMethod]
        public async Task TestBuildIndexV1070()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            Assert.IsTrue(summary.Characters.Contains("The Peismaker"));
        }

        [TestMethod]
        public async Task TestBuildIndexRepeated()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);
            var entries = summary.Entries;
            summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            Assert.AreEqual(entries, summary.Entries);
        }

        [TestMethod]
        public async Task TestBuildIndexStatusCallback()
        {
            var messages = new List<string>();
            Action<string> callback = (msg) => { messages.Add(msg); };

            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true, callback);

            Assert.IsTrue(messages.Count >= 2);
        }


        [TestMethod]
        public async Task TestFindOnName()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.ItemName == "Mythical Ultos' Stormseeker") > 0);
        }

        [TestMethod]
        public async Task TestFindOnCharacterName()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.Owner == "Thorine") > 0);
        }


        [TestMethod]
        public async Task TestLevelRangeLimiterMinMaxLevelBaseline()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 0, MaxLevel = 100, PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.LevelRequirement < 80) > 0);
            Assert.IsTrue(results.Results.Count(x => x.LevelRequirement > 90) > 0);
        }

        [TestMethod]
        public async Task TestLevelRangeLimiterMinMaxLevel()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 80, MaxLevel = 90, PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.LevelRequirement < 80) == 0);
            Assert.IsTrue(results.Results.Count(x => x.LevelRequirement > 90) == 0);
        }

        [TestMethod]
        public async Task TestQualityLimiter()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", ItemQualities = new string[] { "Legendary", "Rare" }, PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Results.Count(x => x.Rarity == "Epic") == 0);
            Assert.IsTrue(results.Results.Count(x => x.Rarity == "Common") == 0);
        }

        [TestMethod]
        public async Task TestQualityLimiterBaseline()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Results.Count(x => x.Rarity == "Epic") > 0);
        }

        [TestMethod]
        public async Task TestFindDuplicates()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindDuplicatesAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Find duplicates", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.ItemName == "Mythical Signet of the Runefather") > 0);
        }

        [TestMethod]
        public async Task TestFindUnequippedWhenIsEquippedIsChecked()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.ItemName == "Fist of Ultos") > 0);
        }

        [TestMethod]
        public async Task TestFindBlueprint()
        {
            var index = new LuceneIndex();
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("blueprint: Cowl of Mogdrogen", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.ItemName == "Blueprint: Cowl of Mogdrogen") > 0);
        }


        [TestMethod]
        public async Task TestFindBlueprint_ExternalSource()
        {
            var index = new Index("formulas_external_1.gst");
            var summary = await index.BuildAsync(null, "Resources/Saves", false, true);

            var results = await index.FindAsync("blueprint: Cowl of Mogdrogen", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Results.Count(x => x.ItemName == "Blueprint: Cowl of Mogdrogen") > 0);
        }
    }
}
