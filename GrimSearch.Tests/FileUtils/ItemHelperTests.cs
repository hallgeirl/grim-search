using GrimSearch.Utils.DBFiles;
using GrimSearch.Utils.CharacterFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class ItemHelperTests
    {
        [TestInitialize]
        public void Initialize()
        {
            StringsCache.Instance.CacheFilename = "Resources/TagsCache.json";
            StringsCache.Instance.Language = "EN";
            StringsCache.Instance.IsDirty = true;
            StringsCache.Instance.LoadAllStrings(null);
            ItemCache.Instance.CacheFilename = "Resources/ItemsCache.json";
        }

        [TestMethod]
        [DataRow(50, 40, 30, 50)]
        [DataRow(20, 50, 30, 50)]
        [DataRow(20, 30, 50, 50)]
        public void GetLevelRequirementUsesHighestItemOrAffixRequirement(
            int itemLevel, int prefixLevel, int suffixLevel, int expectedLevel)
        {
            var itemDef = CreateItemDefinition(itemLevel);
            var prefixDef = CreateItemDefinition(prefixLevel);
            var suffixDef = CreateItemDefinition(suffixLevel);

            var result = ItemHelper.GetLevelRequirement(itemDef, prefixDef, suffixDef);

            Assert.AreEqual(expectedLevel, result);
        }

        [TestMethod]
        [DataRow("ItemArtifactFormula", "Blueprints")]
        [DataRow("ItemDifficultyUnlock", "Difficulty Unlocks")]
        [DataRow("ItemUsableSkill", "Consumables")]
        public void GetItemTypeDisplayNameReturnsHumanReadableName(string itemType, string expectedName)
        {
            Assert.AreEqual(expectedName, ItemHelper.GetItemTypeDisplayName(itemType));
        }

        [TestMethod]
        [DataRow("^kAncient Armor Plate", "Ancient Armor Plate")]
        [DataRow("Prefix ^kAncient Armor Plate", "Prefix Ancient Armor Plate")]
        [DataRow("{^k}Ancient Armor Plate", "Ancient Armor Plate")]
        public void RemoveItemNameFormattingRemovesGrimDawnColorTokens(string itemName, string expectedName)
        {
            Assert.AreEqual(expectedName, ItemHelper.RemoveItemNameFormatting(itemName));
        }

        [TestMethod]
        [DataRow("神話級", "ウルテスの斧", "神話級 ウルテスの斧")]
        [DataRow("Мифический", "Топор Ультоса", "Мифический Топор Ультоса")]
        [DataRow("أسطوري", "فأس أولتوس", "أسطوري فأس أولتوس")]
        public void GetFullItemNameSupportsUnicodeLocalizations(string style, string baseName, string expected)
        {
            var item = new Item();
            var definition = new ItemRaw();
            definition.StringParametersRaw["Class"] = "WeaponMelee_Axe2h";
            definition.StringParametersRaw["itemNameTag"] = "item";
            definition.StringParametersRaw["itemStyleTag"] = "style";
            var strings = new System.Collections.Generic.Dictionary<string, string>
            {
                ["item"] = baseName,
                ["style"] = style
            };

            Assert.AreEqual(expected, ItemHelper.GetFullItemName(item, definition, tag => strings[tag]));
        }

        [TestMethod]
        [DataRow("[ms]Mythischer[fs]Mythische[ns]Mythisches[mp]Mythische[fp]Mythische[np]Mythische", "[ms]Avatar des Chaos", "Mythischer Avatar des Chaos")]
        [DataRow("[ms]Mythischer[fs]Mythische[ns]Mythisches", "[fs]Klinge", "Mythische Klinge")]
        [DataRow("[ms]Mythischer[fs]Mythische[ns]Mythisches", "[ns]Schwert", "Mythisches Schwert")]
        [DataRow("[ms]Anciens[fs]Anciennes[mp]Anciens[fp]Anciennes", "[fp]Lames", "Anciennes Lames")]
        public void GetFullItemNameResolvesGrammaticalVariants(string style, string baseName, string expected)
        {
            var item = new Item();
            var definition = new ItemRaw();
            definition.StringParametersRaw["Class"] = "WeaponMelee_Sword";
            definition.StringParametersRaw["itemNameTag"] = "item";
            definition.StringParametersRaw["itemStyleTag"] = "style";
            var strings = new System.Collections.Generic.Dictionary<string, string>
            {
                ["item"] = baseName,
                ["style"] = style
            };

            Assert.AreEqual(expected, ItemHelper.GetFullItemName(item, definition, tag => strings[tag]));
        }

        [TestMethod]
        public void GetStatsRendersDirectResistanceReductionWithValue()
        {
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["offensiveTotalResistanceReductionPercentMin"] = 25;
            definition.NumericalParametersRaw["offensiveTotalResistanceReductionPercentDurationMin"] = 5;
            definition.NumericalParametersRaw["offensiveTotalResistanceReductionAbsoluteChance"] = 15;
            definition.NumericalParametersRaw["offensivePhysicalResistanceReductionPercentMin"] = 10;

            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.AreEqual(1, stats.Count(x => x == "-25% Reduced target's Resistances"));
            Assert.AreEqual(1, stats.Count(x => x == "-15 Reduced target's Resistances"));
            Assert.AreEqual(1, stats.Count(x => x == "-10% Reduced target's Physical Resistance"));
        }

        [TestMethod]
        public void GetStatsReportsNoResistanceReductionWhenAbsent()
        {
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["offensiveFireMin"] = 10;
            definition.StringParametersRaw["Class"] = "OneShot_PotionHealth";

            var stats = ItemHelper.GetStats(null, definition);

            Assert.IsFalse(stats.Any(x => x.Contains("Reduced target's")));
        }

        [TestMethod]
        public void GetStatsIncludesResistanceReductionFromGrantedSkill()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            var definition = ItemCache.Instance.GetItem("records/items/gearaccessories/medals/c021_medal.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-15 Reduced target's Resistances"));
        }

        [TestMethod]
        public void GetStatsIncludesResistanceReductionFromSkillBuff()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            // The amulet grants a skill that, in turn, applies a buff carrying the resistance reduction.
            var definition = ItemCache.Instance.GetItem("records/items/gearaccessories/necklaces/d218_necklace.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-20 Reduced target's Resistances"));
        }

        [TestMethod]
        public void GetStatsParsesMultiLevelSkillResistanceReduction()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            // The granted skill stores its reduction as a semicolon separated, per-level list.
            var definition = ItemCache.Instance.GetItem("records/items/gearhead/c011_head.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-20 Reduced target's Resistances"));
        }

        [TestMethod]
        public void GetStatsIncludesFlatResistanceReductionFromGrantedSkill()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            // Flat "-X% Resistance" reductions are stored as negative defensive resistance on the granted debuff.
            var definition = ItemCache.Instance.GetItem("records/items/gearrelic/d009_relic.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-10% Pierce Resistance"));
            Assert.IsTrue(stats.Any(x => x == "-10% Chaos Resistance"));
            Assert.IsTrue(stats.Any(x => x == "-10% Elemental Resistance"));
        }

        [TestMethod]
        public void GetStatsIncludesFlatResistanceReductionFromSkillBuff()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            var definition = ItemCache.Instance.GetItem("records/items/gearrelic/b007_relic.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-15% Elemental Resistance"));
        }

        [TestMethod]
        public void GetStatsIncludesCrowdControlResistanceReduction()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            var definition = ItemCache.Instance.GetItem("records/items/lootsets/itemset_c106.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-20% Stun Resistance"));
            Assert.IsTrue(stats.Any(x => x == "-20% Freeze Resistance"));
            Assert.IsTrue(stats.Any(x => x == "-20% Entrapment Resistance"));
        }

        [TestMethod]
        public void GetStatsIncludesLeechResistanceReduction()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            var definition = ItemCache.Instance.GetItem("records/items/gearrelic/c007_relic.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-8% Life Leech Resistance"));
        }

        [TestMethod]
        public void GetStatsIgnoresSelfBuffResistancePenalties()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            // Berserk is a self buff that penalises the player's own resistances; it is not enemy resistance reduction.
            var definition = ItemCache.Instance.GetItem("records/items/gearhead/c002_head.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsFalse(stats.Any(x => x.Contains("Resistance") && x.StartsWith("-")));
        }

        [TestMethod]
        public void GetStatsDoesNotTreatPositiveDefensiveResistanceAsReduction()
        {
            // Items grant positive defensive resistance to the player; that is not a resistance reduction.
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["defensiveElementalResistance"] = 14;

            var stats = ItemHelper.GetStats(null, definition);

            Assert.IsFalse(stats.Any(x => x.Contains("Elemental Resistance")));
        }

        [TestMethod]
        public void GetStatsMapsFlatResistanceReductionDamageTypes()
        {
            ItemCache.Instance.LoadAllItems(null, false, true, _ => { });

            var definition = ItemCache.Instance.GetItem("records/items/gearaccessories/rings/c107_ring.dbr");
            var stats = ItemHelper.GetStats(null, definition)
                .Select(x => x.Replace("{^E}", "").Trim())
                .ToList();

            Assert.IsTrue(stats.Any(x => x == "-10% Vitality Resistance"));
        }

        private static ItemRaw CreateItemDefinition(int levelRequirement)
        {
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["levelRequirement"] = levelRequirement;
            return definition;
        }
    }
}
