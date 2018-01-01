using GDItemSearch.FileUtils.CharacterFiles;
using GDItemSearch.FileUtils.DBFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils
{
    public class Index
    {
        ItemCache _itemCache = ItemCache.Instance;
        StringsCache _stringsCache = StringsCache.Instance;
        List<CharacterFile> _characters = new List<CharacterFile>();
        List<IndexItem> _index = new List<IndexItem>(); //Not really an index though.. for now ;)

        public List<IndexItem> Find(string search, IndexFilter filter)
        {
            var result = _index.Where(x => x.Searchable.Contains(search.ToLower()) && FilterMatch(x, filter));

            if (filter.PageSize != null)
                return result.Take(filter.PageSize.Value).ToList();

            return result.ToList();
        }

        public void ClearCache()
        {
            _itemCache.ClearCache();
            _stringsCache.ClearCache();
        }

        public IndexSummary Build()
        {
            LoadAllCharacters();
            _itemCache.LoadAllItems();
            _stringsCache.LoadAllStrings();
            var summary = BuildIndex();

            return summary;
        }

        private void LoadAllCharacters()
        {
            var charactersDirectory = Path.Combine(Settings.SavesDirectory, "main");
            if (!Directory.Exists(charactersDirectory))
                throw new InvalidOperationException("Saves directory not found: " + charactersDirectory);

            var directories = Directory.EnumerateDirectories(charactersDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var d in directories)
            {
                var characterFile = Path.Combine(d, "player.gdc");
                if (!File.Exists(characterFile))
                    continue;
                var character = new CharacterFile();
                using (var s = File.OpenRead(characterFile))
                {
                    character.Read(s);
                }
                _characters.Add(character);
            }
        }

        private IndexSummary BuildIndex()
        {
            var summary = new IndexSummary();

            foreach (var c in _characters)
            {
                BuildEquippedIndexItems(c, c.Inventory.Equipment, summary);
                BuildEquippedIndexItems(c, c.Inventory.Weapon1, summary);
                BuildEquippedIndexItems(c, c.Inventory.Weapon2, summary);

                foreach (var e in c.Inventory.Sacks)
                {
                    BuildUnequippedIndexItems(c, e.Items.ToArray(), summary);
                }

                foreach (var e in c.Stash.stashPages)
                {
                    BuildUnequippedIndexItems(c, e.items.ToArray(), summary);
                }
            }

            return summary;
        }

        private void BuildEquippedIndexItems(CharacterFile c, Item[] equipment, IndexSummary summary)
        {
            foreach (var e in equipment)
            {
                if (e == null)
                    continue;

                var item = BuildEquippedIndexItem(e, c);
                AddIndexItem(item, summary);
            }
        }

        private void BuildUnequippedIndexItems(CharacterFile c, Item[] items, IndexSummary summary)
        {
            foreach (var e in items)
            {
                if (e == null)
                    continue;

                var item = BuildIndexItem(e, c);
                AddIndexItem(item, summary);
            }
        }

        private void AddIndexItem(IndexItem item, IndexSummary summary)
        {
            if (item != null)
            {
                var itemType = ItemHelper.GetItemType(item.Source);
                if (new string[] {
                    "OneShot_PotionMana",
                    "OneShot_PotionHealth",
                    "OneShot_Scroll",
                    "ItemEnchantment",
                    "QuestItem",
                    "ItemTransmuter",
                    "ItemNote"
                }.Contains(itemType))
                    return;

                summary.Entries++;

                var rarity = ItemHelper.GetItemRarity(item.Source);
                if (rarity != null)
                    summary.ItemRarities.Add(rarity);

                if (itemType != null)
                    summary.ItemTypes.Add(itemType);

                _index.Add(item);
            }
        }

        private IndexItem BuildEquippedIndexItem(Item item, CharacterFile character)
        {
            var i = BuildIndexItem(item, character);

            if (i == null)
                return null;

            i.IsEquipped = true;
            return i;
        }

        private IndexItem BuildIndexItem(Item item, CharacterFile character)
        {
            if (string.IsNullOrEmpty(item.baseName))
                return null;

            var itemDef = _itemCache.GetItem(item.baseName);

            if (itemDef == null)
                return null;

            var indexItem = new IndexItem();
            indexItem.Owner = character.Header.name;
            if (itemDef.NumericalParametersRaw.ContainsKey("levelRequirement"))
                indexItem.LevelRequirement = (int)itemDef.NumericalParametersRaw["levelRequirement"];

            indexItem.Rarity = ItemHelper.GetItemRarity(itemDef);
            indexItem.ItemType = ItemHelper.GetItemType(itemDef);
            indexItem.Source = itemDef;
            indexItem.SourceInstance = item;
            indexItem.ItemStats = ItemHelper.GetStats(item, itemDef).Select(x=>x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "")).ToList();
            indexItem.Searchable = BuildSearchableString(item, itemDef, indexItem.ItemStats);

            return indexItem;
        }

        private bool FilterMatch(IndexItem item, IndexFilter filter)
        {
            if (filter.MaxLevel != null && item.LevelRequirement > filter.MaxLevel)
                return false;

            if (filter.MinLevel != null && item.LevelRequirement < filter.MinLevel)
                return false;

            if (filter.IsEquipped != null && item.IsEquipped != filter.IsEquipped)
                return false;

            if (filter.ItemQualities != null && !filter.ItemQualities.Contains(item.Rarity))
                return false;

            if (filter.ItemTypes != null && !filter.ItemTypes.Contains(item.ItemType))
                return false;

            return true;
        }

        private string BuildSearchableString(Item item, ItemRaw itemDef, List<string> itemStats)
        {
            List<string> searchableStrings = new List<string>();

            searchableStrings.Add(ItemHelper.GetFullItemName(item, itemDef).ToLower());
            searchableStrings.AddRange(itemStats);

            return string.Join(" ", searchableStrings).ToLower();
        }
    }
}
