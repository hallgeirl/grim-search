using GDItemSearch.FileUtils.CharacterFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils.DBFiles
{
    public static class ItemHelper
    {
        public static string GetItemRarity(ItemRaw itemDef)
        {
            if (itemDef.StringParametersRaw.ContainsKey("itemClassification"))
                return itemDef.StringParametersRaw["itemClassification"];

            return null;
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

        public static string GetItemTypeDisplayName(string itemType)
        {
            switch(itemType)
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
                case "WeaponMelee_Sword":
                    return "Swords";
                case "WeaponMelee_Dagger":
                    return "Daggers";
                case "WeaponMelee_Mace":
                    return "Mace";
                case "WeaponMelee_Scepter":
                    return "Scepters";
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

            return GetDamageModifierStats(combinedStats);
        }

        private static Dictionary<string, List<float>> GetCombinedNumericalParameters(Item item, ItemRaw itemDef)
        {
            Dictionary<string, List<float>> combinedStats = new Dictionary<string, List<float>>();
            foreach (var s in itemDef.NumericalParametersRaw)
                combinedStats.Add(s.Key, new List<float>() { s.Value });

            if (!string.IsNullOrEmpty(item.prefixName))
            {
                var prefix = ItemCache.Instance.GetItem(item.prefixName);
                foreach (var s in prefix.NumericalParametersRaw)
                {
                    if (!combinedStats.ContainsKey(s.Key))
                        combinedStats[s.Key] = new List<float>();

                    combinedStats[s.Key].Add(s.Value); 
                }
                    
            }

            if (!string.IsNullOrEmpty(item.suffixName))
            {
                var suffix = ItemCache.Instance.GetItem(item.suffixName);
                foreach (var s in suffix.NumericalParametersRaw)
                {
                    if (!combinedStats.ContainsKey(s.Key))
                        combinedStats[s.Key] = new List<float>();

                    combinedStats[s.Key].Add(s.Value);
                }
            }

            return combinedStats;
        }

        private static List<string> GetDamageModifierStats(Dictionary<string, List<float>> itemParameters)
        {
            //Dictionary<string, string> damageTypesMapping = new Dictionary<string, string>();
            var offensiveStats = new HashSet<KeyValuePair<string, List<float>>>(itemParameters.Where(x => x.Key.StartsWith("offensive")));
            List<string> modifiers = new List<string>();

            //GlobalPercentChanceOfAllTag
            //Parameter name = offensive[Slow]<type>Modifier
            //Tag name = Damage[Duration]Modifier<type> -- format: {%+.0f0}% {^E}<type> Damage
            foreach (var stat in offensiveStats)
            {
                //% modifier
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
                    if (s != null)
                        modifiers.Add(s);
                }

                match = Regex.Match(stat.Key, "offensive([a-zA-Z]+)Min");
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
                        modifiers.Add(s);
                }


                //flat damage
            }

            return modifiers;
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
    }
}
