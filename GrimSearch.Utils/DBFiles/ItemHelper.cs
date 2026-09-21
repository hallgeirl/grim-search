using GrimSearch.Utils.CharacterFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

        public static int GetLevelRequirement(Item item, ItemRaw itemDef)
        {
            var prefixDef = string.IsNullOrEmpty(item?.prefixName)
                ? null
                : ItemCache.Instance.GetItem(item.prefixName);
            var suffixDef = string.IsNullOrEmpty(item?.suffixName)
                ? null
                : ItemCache.Instance.GetItem(item.suffixName);

            return GetLevelRequirement(itemDef, prefixDef, suffixDef);
        }

        internal static int GetLevelRequirement(ItemRaw itemDef, ItemRaw prefixDef, ItemRaw suffixDef)
        {
            return new[] { itemDef, prefixDef, suffixDef }
                .Where(definition => definition != null && definition.NumericalParametersRaw.ContainsKey("levelRequirement"))
                .Select(definition => (int)definition.NumericalParametersRaw["levelRequirement"])
                .DefaultIfEmpty(0)
                .Max();
        }

        public static bool IsFormula(ItemRaw itemDef)
        {
            return GetItemType(itemDef) == "ItemArtifactFormula";
        }

        public static string GetItemIdentity(Item item)
        {
            if (item == null)
                return "";

            return string.Join("|", new[]
            {
                item.baseName,
                item.prefixName,
                item.suffixName
            }.Select(value => value ?? ""));
        }

        public static string GetFullItemName(Item item, ItemRaw itemDef)
        {
            return GetFullItemName(item, itemDef, StringsCache.Instance.GetString);
        }

        internal static string GetFullItemName(Item item, ItemRaw itemDef, Func<string, string> getString)
        {
            string baseName = GetItemBasename(item, itemDef, getString);
            var genderCode = GetGenderCode(baseName);

            var upgradeLevel = ResolveGenderVariant(GetItemUpgradeLevel(itemDef, getString), genderCode);

            List<string> nameComponents = new List<string>();
            nameComponents.Add(upgradeLevel);

            if (!itemDef.NumericalParametersRaw.ContainsKey("hidePrefixName") || itemDef.NumericalParametersRaw["hidePrefixName"] != 0)
                AddAffixNameToNameComponents(item.prefixName, nameComponents, getString, genderCode);

            nameComponents.Add(ResolveGenderVariant(baseName, genderCode));

            if (!itemDef.NumericalParametersRaw.ContainsKey("hideSuffixName") || itemDef.NumericalParametersRaw["hideSuffixName"] != 0)
                AddAffixNameToNameComponents(item.suffixName, nameComponents, getString, genderCode);

            return RemoveItemNameFormatting(string.Join(" ", nameComponents.Where(x => x != null)));
        }

        internal static string RemoveItemNameFormatting(string itemName)
        {
            return itemName == null
                ? null
                : Regex.Replace(itemName, @"(?:\{\^[A-Za-z-]\}|\^[A-Za-z-])", "").Trim();
        }

        internal static string ResolveGenderVariant(string value, string genderCode)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var matches = Regex.Matches(value, @"\[(ms|fs|ns|mp|fp|np)\]", RegexOptions.IgnoreCase);
            if (matches.Count == 0)
                return value;

            var selectedCode = string.IsNullOrEmpty(genderCode)
                ? matches[0].Groups[1].Value
                : genderCode;

            for (var i = 0; i < matches.Count; i++)
            {
                if (!string.Equals(matches[i].Groups[1].Value, selectedCode, StringComparison.OrdinalIgnoreCase))
                    continue;

                var start = matches[i].Index + matches[i].Length;
                var end = i + 1 < matches.Count ? matches[i + 1].Index : value.Length;
                return value.Substring(start, end - start);
            }

            // A malformed or incomplete translation is still more useful without engine metadata.
            var firstStart = matches[0].Index + matches[0].Length;
            var firstEnd = matches.Count > 1 ? matches[1].Index : value.Length;
            return value.Substring(firstStart, firstEnd - firstStart);
        }

        private static string GetGenderCode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var match = Regex.Match(value, @"\[(ms|fs|ns|mp|fp|np)\]", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static void AddAffixNameToNameComponents(string affixPath, List<string> nameComponents, Func<string, string> getString, string genderCode)
        {
            if (!string.IsNullOrEmpty(affixPath))
            {
                var affix = ItemCache.Instance.GetItem(affixPath);
                var affixName = ResolveGenderVariant(GetAffixName(affix, getString), genderCode);
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
                case "ItemArtifactFormula":
                    return "Blueprints";
                case "ItemDifficultyUnlock":
                    return "Difficulty Unlocks";
                case "ItemUsableSkill":
                    return "Consumables";
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
                case "WeaponMelee_Spear2h":
                    return "Two-Handed Spears";
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
        /// <param name="itemStringParameters"></param>
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
                // Resistance reduction is rendered separately so that the reduction value is included. Without this
                // guard the generic flat damage handler would render it without a value.
                if (GetResistanceReductionTag(stat.Key) != null)
                    continue;

                AddPercentageDamageModifier(modifiers, stat);
                AddFlatDamageModifier(modifiers, stat);
                AddAllSkillsModifier(modifiers, stat);
                AddRetaliationFlatDamageModifier(modifiers, stat);
                AddRetaliationPercentageDamageModifier(modifiers, stat);
            }

            var resistanceReductions = new Dictionary<string, float>();
            var resistanceReductionRanks = new Dictionary<string, int>();
            CollectResistanceReductions(resistanceReductions, resistanceReductionRanks, itemNumericalParameters, itemStringParameters);

            foreach (var stat in itemStringParameters)
            {
                AddMasteryModifier(modifiers, stat);
                AddSkillModifier(modifiers, stat);

                // Items can grant skills (e.g. procs, relic auras and component abilities) that reduce enemy
                // resistances. Those skills carry the actual parameters, so they have to be resolved as well.
                CollectAttachedSkillResistanceReductions(resistanceReductions, resistanceReductionRanks, stat);
            }

            foreach (var reduction in resistanceReductions)
            {
                var rendered = reduction.Key.StartsWith("Defense", StringComparison.Ordinal)
                    ? RenderDefensiveResistanceReduction(reduction.Key, reduction.Value)
                    : RenderResistanceReduction(reduction.Key, reduction.Value);
                if (rendered != null)
                    modifiers.Add(rendered);
            }

            return modifiers;
        }

        #region Resistance reduction

        private static readonly string[] ResistanceReductionDamageTypes = { "Total", "Physical", "Elemental" };
        private static readonly string[] ResistanceReductionKinds = { "Percent", "Absolute" };
        private static readonly string[] AttachedSkillParameterNames = { "itemSkillName", "skillName" };

        // The flat "-X% <type> Resistance" reduction is stored on skills (and their buffs) as a negative defensive
        // resistance. These are the parameter/tag pairs that represent it.
        private static readonly Dictionary<string, string> DefensiveResistanceReductionTags = new Dictionary<string, string>
        {
            ["defensiveElementalResistance"] = "DefenseElementalResistance",
            ["defensivePhysical"] = "DefensePhysical",
            ["defensivePierce"] = "DefensePierce",
            ["defensiveFire"] = "DefenseFire",
            ["defensiveCold"] = "DefenseCold",
            ["defensiveLightning"] = "DefenseLightning",
            ["defensivePoison"] = "DefensePoison",
            ["defensiveAether"] = "DefenseAether",
            ["defensiveChaos"] = "DefenseChaos",
            ["defensiveBleeding"] = "DefenseBleeding",
            ["defensiveLife"] = "DefenseLife",
            ["defensiveStun"] = "DefenseStunNegative",
            ["defensiveFreeze"] = "DefenseFreezeNegative",
            ["defensiveKnockdown"] = "DefenseKnockdownNegative",
            ["defensivePetrify"] = "DefensePetrifyNegative",
            ["defensiveTrap"] = "DefenseTrapNegative",
            ["defensiveConfusion"] = "DefenseConfusion",
            ["defensiveSleep"] = "tagDefenseSleep",
            ["defensiveSlowLifeLeach"] = "DefenseLifeLeach",
            ["defensiveSlowManaLeach"] = "DefenseManaLeach"
        };

        /// <summary>
        /// Maps a resistance reduction parameter (e.g. offensiveTotalResistanceReductionPercentMin) to the
        /// localization tag that describes it (e.g. DamageTotalResistanceReductionPercent), or null if the
        /// parameter is not a resistance reduction stat.
        /// </summary>
        private static string GetResistanceReductionTag(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName) || !parameterName.StartsWith("offensive", StringComparison.Ordinal))
                return null;

            foreach (var damageType in ResistanceReductionDamageTypes)
            {
                foreach (var kind in ResistanceReductionKinds)
                {
                    if (parameterName.StartsWith($"offensive{damageType}ResistanceReduction{kind}", StringComparison.Ordinal))
                        return $"Damage{damageType}ResistanceReduction{kind}";
                }
            }

            return null;
        }

        /// <summary>
        /// Ranks resistance reduction parameters so that the actual reduction amount is preferred over the
        /// duration or chance parameters when several of them describe the same reduction.
        /// </summary>
        private static int GetResistanceReductionRank(string parameterName)
        {
            if (parameterName.EndsWith("DurationMin", StringComparison.Ordinal))
                return 3;

            if (parameterName.EndsWith("Min", StringComparison.Ordinal))
                return 1;

            return 2;
        }

        private static void AddResistanceReduction(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            string tagName,
            float value,
            string parameterName)
        {
            var rank = GetResistanceReductionRank(parameterName);
            if (!collected.ContainsKey(tagName) || rank < ranks[tagName])
            {
                collected[tagName] = value;
                ranks[tagName] = rank;
            }
        }

        private static float? ParseFirstParameterValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            // Some parameters (notably skills that scale per level) are stored as a semicolon separated list.
            foreach (var part in value.Split(';'))
            {
                if (float.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;
            }

            return null;
        }

        private static void CollectResistanceReductions(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            IReadOnlyDictionary<string, List<float>> numericalParameters,
            IReadOnlyDictionary<string, List<string>> stringParameters)
        {
            foreach (var stat in numericalParameters)
            {
                var tagName = GetResistanceReductionTag(stat.Key);
                if (tagName != null)
                    AddResistanceReduction(collected, ranks, tagName, stat.Value.Sum(), stat.Key);
            }

            foreach (var stat in stringParameters)
            {
                var tagName = GetResistanceReductionTag(stat.Key);
                if (tagName == null)
                    continue;

                foreach (var rawValue in stat.Value)
                {
                    var value = ParseFirstParameterValue(rawValue);
                    if (value != null)
                    {
                        AddResistanceReduction(collected, ranks, tagName, value.Value, stat.Key);
                        break;
                    }
                }
            }
        }

        private static void CollectResistanceReductions(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            IReadOnlyDictionary<string, float> numericalParameters,
            IReadOnlyDictionary<string, string> stringParameters)
        {
            foreach (var stat in numericalParameters)
            {
                var tagName = GetResistanceReductionTag(stat.Key);
                if (tagName != null)
                    AddResistanceReduction(collected, ranks, tagName, stat.Value, stat.Key);
            }

            foreach (var stat in stringParameters)
            {
                var tagName = GetResistanceReductionTag(stat.Key);
                if (tagName == null)
                    continue;

                var value = ParseFirstParameterValue(stat.Value);
                if (value != null)
                    AddResistanceReduction(collected, ranks, tagName, value.Value, stat.Key);
            }
        }

        private static void CollectAttachedSkillResistanceReductions(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            KeyValuePair<string, List<string>> stat)
        {
            if (stat.Value == null || !AttachedSkillParameterNames.Contains(stat.Key))
                return;

            foreach (var skillPath in stat.Value)
            {
                var skill = ItemCache.Instance.GetItem(skillPath);
                CollectSkillResistanceReductions(collected, ranks, skill, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { skillPath });
            }
        }

        private static void CollectSkillResistanceReductions(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            ItemRaw skill,
            HashSet<string> visitedSkillPaths)
        {
            if (skill == null)
                return;

            CollectResistanceReductions(collected, ranks, skill.NumericalParametersRaw, skill.StringParametersRaw);

            // Flat negative defensive resistance is only a resistance reduction when the skill is a debuff applied to
            // enemies. Self buffs (which penalise the player's own resistances) and passives must not be treated as RR.
            if (IsDebuffSkill(skill))
                CollectDefensiveResistanceReductions(collected, ranks, skill.NumericalParametersRaw, skill.StringParametersRaw);

            // Some item skills are just triggers that apply a buff. The buff is where the actual stats live.
            if (skill.StringParametersRaw.TryGetValue("buffSkillName", out var buffSkillName)
                && !string.IsNullOrEmpty(buffSkillName)
                && visitedSkillPaths.Add(buffSkillName))
            {
                CollectSkillResistanceReductions(collected, ranks, ItemCache.Instance.GetItem(buffSkillName), visitedSkillPaths);
            }
        }

        private static bool IsDebuffSkill(ItemRaw skill)
        {
            return ContainsDebuffMarker(skill.StringParametersRaw.TryGetValue("templateName", out var template) ? template : null)
                || ContainsDebuffMarker(skill.StringParametersRaw.TryGetValue("Class", out var className) ? className : null);
        }

        private static bool ContainsDebuffMarker(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.IndexOf("debuf", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("contageous", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void CollectDefensiveResistanceReductions(
            Dictionary<string, float> collected,
            Dictionary<string, int> ranks,
            IReadOnlyDictionary<string, float> numericalParameters,
            IReadOnlyDictionary<string, string> stringParameters)
        {
            foreach (var stat in numericalParameters)
            {
                if (stat.Value >= 0 || !DefensiveResistanceReductionTags.TryGetValue(stat.Key, out var tagName))
                    continue;

                AddResistanceReduction(collected, ranks, tagName, stat.Value, stat.Key);
            }

            foreach (var stat in stringParameters)
            {
                if (!DefensiveResistanceReductionTags.TryGetValue(stat.Key, out var tagName))
                    continue;

                var value = ParseFirstParameterValue(stat.Value);
                if (value == null || value.Value >= 0)
                    continue;

                AddResistanceReduction(collected, ranks, tagName, value.Value, stat.Key);
            }
        }

        private static string FormatReductionValue(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string RenderResistanceReduction(string tagName, float value)
        {
            var template = StringsCache.Instance.GetString(tagName);
            if (string.IsNullOrEmpty(template))
                return null;

            // The game localizations prepend the reduction value to the tag text (the tag itself starts with "%" for
            // percentage based reductions and a space for absolute ones).
            return "-" + FormatReductionValue(value) + template;
        }

        private static string RenderDefensiveResistanceReduction(string tagName, float value)
        {
            var template = StringsCache.Instance.GetString(tagName);
            if (string.IsNullOrEmpty(template))
                return null;

            // Flat resistance reductions reuse the defensive resistance tags, which contain the value placeholder.
            return template
                .Replace("{%+.0f0}", FormatReductionValue(value))
                .Replace("{%.0f0}", FormatReductionValue(value))
                .Replace("{%t0}", FormatReductionValue(value));
        }

        #endregion

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

        private static string GetAffixName(ItemRaw itemDef, Func<string, string> getString)
        {
            if (itemDef == null || !itemDef.StringParametersRaw.ContainsKey("lootRandomizerName"))
                return null;

            var tagName = itemDef.StringParametersRaw["lootRandomizerName"];

            return getString(tagName);
        }

        private static string GetItemBasename(Item item, ItemRaw itemDef, Func<string, string> getString)
        {
            if (itemDef.StringParametersRaw.ContainsKey("itemNameTag"))
                return getString(itemDef.StringParametersRaw["itemNameTag"]);

            if (itemDef.StringParametersRaw["Class"] != "ItemRelic" && itemDef.StringParametersRaw.ContainsKey("FileDescription"))
                return itemDef.StringParametersRaw["FileDescription"];

            if (itemDef.StringParametersRaw.ContainsKey("description"))
                return getString(itemDef.StringParametersRaw["description"]);

            return "";
        }

        private static string GetItemUpgradeLevel(ItemRaw itemDef, Func<string, string> getString)
        {
            if (itemDef.StringParametersRaw.ContainsKey("itemStyleTag") && !string.IsNullOrEmpty(itemDef.StringParametersRaw["itemStyleTag"]))
                return getString(itemDef.StringParametersRaw["itemStyleTag"]);

            return null;
        }

        private static bool IsComponentBlueprint(ItemRaw itemDef)
        {
            if (!IsFormula(itemDef))
                return false;

            var targetItem = ItemCache.Instance.GetItem(itemDef.StringParametersRaw["artifactName"]);
            if (targetItem == null)
                return false;

            return targetItem.StringParametersRaw["Class"] == "ItemRelic";
        }
    }
}
