﻿using GrimSearch.Utils;
using GrimSearch.Utils.DBFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Tests.FileUtils
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
        public async Task TestBuildIndex()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            Assert.IsTrue(summary.ItemRarities.Contains("Legendary"));
            Assert.IsTrue(summary.ItemRarities.Contains("Epic"));
            Assert.IsTrue(summary.ItemRarities.Contains("Rare"));

            Assert.IsTrue(summary.ItemTypes.Contains("WeaponMelee_Axe2h"));
        }

        [TestMethod]
        public async Task TestBuildIndexRepeated()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");
            var entries = summary.Entries;
            summary = await index.BuildAsync(null, "Resources\\Saves");

            Assert.AreEqual(entries, summary.Entries);
        }

        [TestMethod]
        public async Task TestBuildIndexStatusCallback()
        {
            var messages = new List<string>();
            Action<string> callback = (msg) => { messages.Add(msg); };

            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves", callback);

            Assert.IsTrue(messages.Count >= 2);
        }


        [TestMethod]
        public async Task TestFindOnName()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Mythical Ultos' Stormseeker") > 0);
        }

        [TestMethod]
        public async Task TestFindOnCharacterName()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Owner == "Thorine") > 0);
        }


        [TestMethod]
        public async Task TestLevelRangeLimiterMinMaxLevelBaseline()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 0, MaxLevel = 100, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.LevelRequirement < 80) > 0);
            Assert.IsTrue(results.Count(x => x.LevelRequirement > 90) > 0);
        }

        [TestMethod]
        public async Task TestLevelRangeLimiterMinMaxLevel()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", MinLevel = 80, MaxLevel = 90, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.LevelRequirement < 80) == 0);
            Assert.IsTrue(results.Count(x => x.LevelRequirement > 90) == 0);
        }

        [TestMethod]
        public async Task TestQualityLimiter()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", ItemQualities = new string[] { "Legendary", "Rare" }, PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Epic") == 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Common") == 0);
        }

        [TestMethod]
        public async Task TestQualityLimiterBaseline()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.Rarity == "Legendary") > 0);
            Assert.IsTrue(results.Count(x => x.Rarity == "Epic") > 0);
        }

        [TestMethod]
        public async Task TestFindDuplicates()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindDuplicatesAsync("thorine", new IndexFilter() { IncludeEquipped = true, SearchMode = "Find duplicates", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Mythical Signet of the Runefather") > 0);
        }

        [TestMethod]
        public async Task TestFindUnequippedWhenIsEquippedIsChecked()
        {
            var index = new Index();
            var summary = await index.BuildAsync(null, "Resources\\Saves");

            var results = await index.FindAsync("ulTos", new IndexFilter() { IncludeEquipped = true, SearchMode = "Regular", PageSize = 1000 });

            Assert.IsTrue(results.Count(x => x.ItemName == "Fist of Ultos") > 0);
        }
    }
}