using GDItemSearch.FileUtils.CharacterFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            nameComponents.Add(baseName);

            return string.Join(" ", nameComponents.Where(x=>x != null));
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
