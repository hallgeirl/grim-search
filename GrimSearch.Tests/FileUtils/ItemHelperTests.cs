using GrimSearch.Utils.DBFiles;
using GrimSearch.Utils.CharacterFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class ItemHelperTests
    {
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

        private static ItemRaw CreateItemDefinition(int levelRequirement)
        {
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["levelRequirement"] = levelRequirement;
            return definition;
        }
    }
}
