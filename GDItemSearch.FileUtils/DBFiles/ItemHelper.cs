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
