using GrimSearch.Utils.DBFiles;
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

        private static ItemRaw CreateItemDefinition(int levelRequirement)
        {
            var definition = new ItemRaw();
            definition.NumericalParametersRaw["levelRequirement"] = levelRequirement;
            return definition;
        }
    }
}
