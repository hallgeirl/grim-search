using GrimSearch.Utils.CharacterFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public static class ItemHelper
    {
        public static string GetItemRarity(ItemRaw itemDef)
        {
            string rarity = null;

            if (itemDef.StringParametersRaw.ContainsKey("itemClassification"))
                rarity = itemDef.StringParametersRaw["itemClassification"];

            if (IsComponentBlueprint(itemDef))
                rarity = "Component Blueprint";

            return rarity;
        }

        public static string GetItemType(ItemRaw itemDef)
        {
            if (itemDef.StringParametersRaw.ContainsKey("Class"))
                return itemDef.StringParametersRaw["Class"];

            return null;
        }

        public static string GetFullItemName(Item item, ItemRaw itemDef)
        {
            string baseName = GetItemBasename(item, itemDef);

            var upgradeLevel = GetItemUpgradeLevel(itemDef);

            List<string> nameComponents = new List<string>();
            nameComponents.Add(upgradeLevel);

            if (!itemDef.NumericalParametersRaw.ContainsKey("hidePrefixName") || itemDef.NumericalParametersRaw["hidePrefixName"] != 0)
                AddAffixNameToNameComponents(item.prefixName, nameComponents);

            nameComponents.Add(baseName);

            if (!itemDef.NumericalParametersRaw.ContainsKey("hideSuffixName") || itemDef.NumericalParametersRaw["hideSuffixName"] != 0)
                AddAffixNameToNameComponents(item.suffixName, nameComponents);

            return string.Join(" ", nameComponents.Where(x => x != null));
        }

        private static void AddAffixNameToNameComponents(string affixPath, List<string> nameComponents)
        {
            if (!string.IsNullOrEmpty(affixPath))
            {
                var affix = ItemCache.Instance.GetItem(affixPath);
                var affixName = GetAffixName(affix);
                if (!string.IsNullOrEmpty(affixName))
                    nameComponents.Add(affixName);
            }
        }

        //Returns the item definition that is used for stats (relevant in case of blueprints, where the item itself doesn't have stats, but the crafted item does)
        public static string GetItemStatSource(ItemRaw itemDef)
        {
            if (GetItemType(itemDef) == "ItemArtifactFormula" && itemDef.StringParametersRaw.ContainsKey("artifactName"))
            {
                return itemDef.StringParametersRaw["artifactName"];
            }

            return null;
        }

        public static string GetItemTypeDisplayName(string itemType)
        {
            switch (itemType)
            {
                case "ArmorProtective_Head":
                    return "Helm";
                case "ArmorProtective_Chest":
                    return "Chest Armor";
                case "ArmorProtective_Feet":
                    return "Boots";
                case "ArmorProtective_Legs":
                    return "Leg armor";
                case "ArmorProtective_Hands":
                    return "Gloves";
                case "ArmorJewelry_Amulet":
                    return "Amulets";
                case "ArmorJewelry_Ring":
                    return "Rings";
                case "ArmorProtective_Waist":
                    return "Belts";
                case "ArmorProtective_Shoulders":
                    return "Shoulders";
                case "ArmorJewelry_Medal":
                    return "Medals";
                case "ItemArtifact":
                    return "Relics";
                case "ItemRelic":
                    return "Components";
                case "WeaponMelee_Axe":
                    return "Axes";
                case "WeaponMelee_Sword":
                    return "Swords";
                case "WeaponMelee_Dagger":
                    return "Daggers";
                case "WeaponMelee_Mace":
                    return "Mace";
                case "WeaponMelee_Scepter":
                    return "Scepters";
                case "WeaponArmor_Offhand":
                    return "Off-hand";
                case "WeaponArmor_Shield":
                    return "Shields";
                case "WeaponMelee_Mace2h":
                    return "Two-Handed Maces";
                case "WeaponMelee_Axe2h":
                    return "Two-Handed Axes";
                case "WeaponMelee_Sword2h":
                    return "Two-Handed Swords";
                case "WeaponHunting_Ranged1h":
                    return "One-Handed Ranged";
                case "WeaponHunting_Ranged2h":
                    return "Two-Handed Ranged";
                case "ItemDevotionReset":
                    return "Devotion Reset Potion";
                case "ItemAttributeReset":
                    return "Attribute Reset Potion";
                case "ItemFactionBooster":
                    return "Faction Boosters";
                case "ItemFactionWarrant":
                    return "Faction Warrants";
                default:
                    return itemType;
            }
        }

        //"Cheat sheet" for item parameters:
        //offensiveAetherGlobal: Damage type Aether is given a chance to be afflicted among other global damage types. For instance - prismatic eviscerator: "10% chance of: X, Y or Z" (where X, Y or Z are all global)
        //offensiveAetherChance: Chance component of flat damage(?)
        //offensiveAetherMin X/offensiveAetherMax Y: flat damage (between X and Y)
        //offensiveAetherModifier X: +X% to Aether damage
        //offensiveAetherModifierChance Y: Y% chance of +X% to Aether damage
        //offensiveAetherXOR: Who knows??
        public static List<string> GetStats(Item item, ItemRaw itemDef)
        {
            var combinedStats = GetCombinedNumericalParameters(item, itemDef);
            var combinedStringParameters = GetCombinedStringParameters(item, itemDef);

            return GetStatsCore(combinedStats, combinedStringParameters);
        }

        /// <summary>
        /// Returns all numerical stat parameters, combining the item's and affixes.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemDef"></param>
        /// <returns></returns>
        private static Dictionary<string, List<float>> GetCombinedNumericalParameters(Item item, ItemRaw itemDef)
        {
            Dictionary<string, List<float>> combinedStats = new Dictionary<string, List<float>>();
            foreach (var s in itemDef.NumericalParametersRaw)
                combinedStats.Add(s.Key, new List<float>() { s.Value });

            if (!string.IsNullOrEmpty(item?.prefixName))
            {
                AddNumericalStatsFromItemOrSuffix(item.prefixName, combinedStats);
            }

            if (!string.IsNullOrEmpty(item?.suffixName))
            {
                AddNumericalStatsFromItemOrSuffix(item.suffixName, combinedStats);
            }

            return combinedStats;
        }

        /// <summary>
        /// Returns all numerical stat parameters, combining the item's and affixes.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemDef"></param>
        /// <returns></returns>
        private static Dictionary<string, List<string>> GetCombinedStringParameters(Item item, ItemRaw itemDef)
        {
            Dictionary<string, List<string>> combinedStats = new Dictionary<string, List<string>>();
            foreach (var s in itemDef.StringParametersRaw)
                combinedStats.Add(s.Key, new List<string>() { s.Value });

            if (!string.IsNullOrEmpty(item?.prefixName))
            {
                AddStringStatsFromItemOrSuffix(item.prefixName, combinedStats);
            }

            if (!string.IsNullOrEmpty(item?.suffixName))
            {
                AddStringStatsFromItemOrSuffix(item.suffixName, combinedStats);
            }

            return combinedStats;
        }

        private static void AddNumericalStatsFromItemOrSuffix(string recordName, Dictionary<string, List<float>> combinedStats)
        {
            var prefix = ItemCache.Instance.GetItem(recordName);
            foreach (var s in prefix.NumericalParametersRaw)
            {
                if (!combinedStats.ContainsKey(s.Key))
                    combinedStats[s.Key] = new List<float>();

                combinedStats[s.Key].Add(s.Value);
            }
        }

        private static void AddStringStatsFromItemOrSuffix(string recordName, Dictionary<string, List<string>> combinedStats)
        {
            var prefix = ItemCache.Instance.GetItem(recordName);
            foreach (var s in prefix.StringParametersRaw)
            {
                if (!combinedStats.ContainsKey(s.Key))
                    combinedStats[s.Key] = new List<string>();

                combinedStats[s.Key].Add(s.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemNumericalParameters"></param>
        /// <returns></returns>
        private static List<string> GetStatsCore(Dictionary<string, List<float>> itemNumericalParameters, Dictionary<string, List<string>> itemStringParameters)
        {
            var offensiveStats = new HashSet<KeyValuePair<string, List<float>>>(itemNumericalParameters.Where(x => x.Key.StartsWith("offensive")));
            List<string> modifiers = new List<string>();

            //GlobalPercentChanceOfAllTag
            //Parameter name = offensive[Slow]<type>Modifier
            //Tag name = Damage[Duration]Modifier<type> -- format: {%+.0f0}% {^E}<type> Damage
            foreach (var stat in itemNumericalParameters)
            {
                AddPercentageDamageModifier(modifiers, stat);
                AddFlatDamageModifier(modifiers, stat);
                AddAllSkillsModifier(modifiers, stat);
                AddRetaliationFlatDamageModifier(modifiers, stat);
                AddRetaliationPercentageDamageModifier(modifiers, stat);
            }

            foreach (var stat in itemStringParameters)
            {
                AddMasteryModifier(modifiers, stat);
                AddSkillModifier(modifiers, stat);
            }

            return modifiers;
        }

        private static void AddMasteryModifier(List<string> modifiers, KeyValuePair<string, List<string>> stat)
        {
            var match = Regex.Match(stat.Key, "augmentMasteryName[0-9]+");
            if (match.Success && stat.Value != null && stat.Value.Count > 0)
            {
                ItemRaw augmentMastery = ItemCache.Instance.GetItem(stat.Value.First());

                var s = StringsCache.Instance.GetString(augmentMastery.StringParametersRaw["skillDisplayName"]);
                if (s != null)
                    modifiers.Add("+ to all skills in " + s);
            }
        }

        private static void AddSkillModifier(List<string> modifiers, KeyValuePair<string, List<string>> stat)
        {
            var match = Regex.Match(stat.Key, "augmentSkillName[0-9]+");
            if (match.Success && stat.Value != null && stat.Value.Count > 0)
            {
                ItemRaw augmentSkill = ItemCache.Instance.GetItem(stat.Value.First());

                string skillName = null;
                if (augmentSkill.StringParametersRaw.ContainsKey("skillDisplayName"))
                    skillName = augmentSkill.StringParametersRaw["skillDisplayName"];
                else if (augmentSkill.StringParametersRaw.ContainsKey("buffSkillName"))
                {
                    ItemRaw actualSkill = ItemCache.Instance.GetItem(augmentSkill.StringParametersRaw["buffSkillName"]);
                    skillName = actualSkill.StringParametersRaw["skillDisplayName"];
                }

                if (skillName == null)
                    return;

                var s = StringsCache.Instance.GetString(skillName);
                if (s != null)
                    modifiers.Add("+ to " + s);
            }
        }

        private static void AddAllSkillsModifier(List<string> modifiers, KeyValuePair<string, List<float>> stat)
        {
            if (stat.Key == "augmentAllLevel")
                modifiers.Add("+ to all Skills");
        }


        private static void AddFlatDamageModifier(List<string> modifiers, KeyValuePair<string, List<float>> stat)
        {
            var match = Regex.Match(stat.Key, "offensive([a-zA-Z]+)Min");
            if (match.Success)
            {
                string tagName = "";

                var matchedDmg = match.Groups[1].Value;
                if (matchedDmg.StartsWith("Slow"))
                {
                    tagName = "DamageDuration" + matchedDmg.Replace("Slow", "");
                }
                else
                {
                    tagName = "Damage" + matchedDmg;
                }

                var s = StringsCache.Instance.GetString(tagName);
                if (s != null)
                {
                    var statWithStringAndNumber = s.Replace("{%t0}", $"{stat.Value.Sum()}");
                    modifiers.Add(statWithStringAndNumber);
                }
            }
        }

        private static void AddRetaliationFlatDamageModifier(List<string> modifiers, KeyValuePair<string, List<float>> stat)
        {
            var match = Regex.Match(stat.Key, "retaliation([a-zA-Z]+)Min");
            if (match.Success)
            {
                string tagName = "";

                var matchedDmg = match.Groups[1].Value;
                if (matchedDmg.StartsWith("Slow"))
                {
                    tagName = "RetaliationDuration" + matchedDmg.Replace("Slow", "");
                }
                else
                {
                    tagName = "Retaliation" + matchedDmg;
                }

                var s = StringsCache.Instance.GetString(tagName);
                if (s != null)
                {
                    var statWithStringAndNumber = s.Replace("{%t0}", $"{stat.Value.Sum()}");
                    modifiers.Add(statWithStringAndNumber);
                }
            }
        }

        private static void AddRetaliationPercentageDamageModifier(List<string> modifiers, KeyValuePair<string, List<float>> stat)
        {
            var match = Regex.Match(stat.Key, "retaliation([a-zA-Z]+)Modifier");
            if (match.Success)
            {
                string tagName = "";

                var matchedDmg = match.Groups[1].Value;
                if (matchedDmg == "TotalDamage")
                {
                    tagName = "tagRetaliationModifierTotalDamage";
                }
                else if (matchedDmg.StartsWith("Slow"))
                {
                    tagName = "RetaliationDurationModifier" + matchedDmg.Replace("Slow", "");
                }
                else
                {
                    tagName = "RetaliationModifier" + matchedDmg;
                }

                var s = StringsCache.Instance.GetString(tagName);
                if (s != null)
                {
                    var statWithStringAndNumber = s.Replace("{%+.0f0}", $"+{stat.Value.Sum()}");
                    modifiers.Add(statWithStringAndNumber);
                }
            }
        }

        private static void AddPercentageDamageModifier(List<string> modifiers, KeyValuePair<string, List<float>> stat)
        {
            var match = Regex.Match(stat.Key, "offensive([a-zA-Z]+)Modifier");
            if (match.Success)
            {
                string tagName = "";

                var matchedDmg = match.Groups[1].Value;
                if (matchedDmg.StartsWith("Slow"))
                {
                    tagName = "DamageDurationModifier" + matchedDmg.Replace("Slow", "");
                }
                else
                {
                    tagName = "DamageModifier" + matchedDmg;
                }

                var s = StringsCache.Instance.GetString(tagName);
                if (s == null)
                {
                    s = StringsCache.Instance.GetString($"tag{tagName}");
                }
                if (s != null)
                {
                    var statWithStringAndNumber = s.Replace("{%+.0f0}", $"+{stat.Value.Sum()}");
                    modifiers.Add(statWithStringAndNumber);
                }
            }
        }

        private static string GetAffixName(ItemRaw itemDef)
        {
            if (!itemDef.StringParametersRaw.ContainsKey("lootRandomizerName"))
                return null;

            var tagName = itemDef.StringParametersRaw["lootRandomizerName"];

            return StringsCache.Instance.GetString(tagName);
        }

        private static string GetItemBasename(Item item, ItemRaw itemDef)
        {
            if (itemDef.StringParametersRaw.ContainsKey("itemNameTag"))
                return StringsCache.Instance.GetString(itemDef.StringParametersRaw["itemNameTag"]);

            if (itemDef.StringParametersRaw["Class"] != "ItemRelic" && itemDef.StringParametersRaw.ContainsKey("FileDescription"))
                return itemDef.StringParametersRaw["FileDescription"];

            if (itemDef.StringParametersRaw.ContainsKey("description"))
                return StringsCache.Instance.GetString(itemDef.StringParametersRaw["description"]);

            return "";
        }

        private static string GetItemUpgradeLevel(ItemRaw itemDef)
        {
            if (itemDef.StringParametersRaw.ContainsKey("itemStyleTag") && !string.IsNullOrEmpty(itemDef.StringParametersRaw["itemStyleTag"]))
                return StringsCache.Instance.GetString(itemDef.StringParametersRaw["itemStyleTag"]);

            return null;
        }

        private static bool IsComponentBlueprint(ItemRaw itemDef)
        {
            if (GetItemType(itemDef) != "ItemArtifactFormula")
                return false;

            var targetItem = ItemCache.Instance.GetItem(itemDef.StringParametersRaw["artifactName"]);
            if (targetItem == null)
                return false;

            return targetItem.StringParametersRaw["Class"] == "ItemRelic";
        }
    }
}
