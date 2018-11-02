using GDItemSearch.Utils.CharacterFiles;
using GDItemSearch.Utils.DBFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils
{
    public class Index
    {
        public Index()
        {
        }

        public Index(ItemCache itemCache, StringsCache stringsCache)
        {
            _itemCache = itemCache;
            _stringsCache = stringsCache;
        }

        ItemCache _itemCache = ItemCache.Instance;
        StringsCache _stringsCache = StringsCache.Instance;
        List<CharacterFile> _characters = new List<CharacterFile>();
        List<IndexItem> _index = new List<IndexItem>(); //Not really an index though.. for now ;)

        public List<IndexItem> Find(string search, IndexFilter filter)
        {
            search = search ?? "";

            var result = _index.Where(x => x.Searchable.Contains(search.ToLower()) && FilterMatch(x, filter));

            if (filter.PageSize != null)
                return result.Take(filter.PageSize.Value).ToList();

            return result.ToList();
        }

        public List<IndexItem> FindDuplicates(string search, IndexFilter filter)
        {
            search = search ?? "";

            var characterItems = _index.Where(x => x.Owner.ToLower() == search.ToLower() && FilterMatch(x, filter));
            List<IndexItem> results = new List<IndexItem>();

            foreach (var item in characterItems)
            {
                var itemName = ItemHelper.GetFullItemName(item.SourceInstance, item.Source);
                var dupe = _index.Where(x => x.ItemName.ToLower() == itemName.ToLower() && x.Owner.ToLower() != search.ToLower());

                if (dupe.Count() > 0)
                {
                    results.Add(item);
                    item.DuplicatesOnCharacters = dupe.Select(x => x.Owner).ToList();
                }
            }
            return results.OrderBy(x=>x.Bag).ToList();
        }

        public void ClearCache()
        {
            _itemCache.ClearCache();
            _stringsCache.ClearCache();
        }

        public IndexSummary Build(string grimDawnDirectory, string grimDawnSavesDirectory)
        {
            LoadAllCharacters(grimDawnSavesDirectory);

            _itemCache.LoadAllItems(grimDawnDirectory);
            _stringsCache.LoadAllStrings(grimDawnDirectory);
            var summary = BuildIndex();


            return summary;
        }

        private void LoadAllCharacters(string grimDawnSavesDirectory)
        {
            _characters.Clear();

            var charactersDirectory = Path.Combine(grimDawnSavesDirectory, "main");
            if (!Directory.Exists(charactersDirectory))
                throw new InvalidOperationException("Saves directory not found: " + charactersDirectory);

            var directories = Directory.EnumerateDirectories(charactersDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var d in directories)
            {
                var characterFile = Path.Combine(d, "player.gdc");
                if (!File.Exists(characterFile))
                    continue;
                var character = new CharacterFile();
                try
                {
                    using (var s = File.OpenRead(characterFile))
                    {
                        character.Read(s);
                    }
                    _characters.Add(character);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }

            LoadTransferStashAsCharacter(grimDawnSavesDirectory);
            LoadBlueprintsAsCharacter(grimDawnSavesDirectory);
        }

        private void LoadTransferStashAsCharacter(string grimDawnSavesDirectory)
        {
            var transferStashFile = Path.Combine(grimDawnSavesDirectory, "transfer.gst");
            var transferStash = new TransferStashFile();
            using (var s = File.OpenRead(transferStashFile))
            {
                transferStash.Read(s);
            }

            _characters.Add(transferStash.ToCharacterFile());
        }

        private void LoadBlueprintsAsCharacter(string grimDawnSavesDirectory)
        {
            var recipesFilePath = Path.Combine(grimDawnSavesDirectory, "formulas.gst");
            var recipes = new BlueprintFile();
            using (var s = File.OpenRead(recipesFilePath))
            {
                recipes.Read(s);
            }

            _characters.Add(recipes.ToCharacterFile());
        }

        private IndexSummary BuildIndex()
        {
            _index.Clear();
            var summary = new IndexSummary();

            foreach (var c in _characters)
            {
                BuildEquippedIndexItems(c, c.Inventory.Equipment, summary);
                BuildEquippedIndexItems(c, c.Inventory.Weapon1, summary);
                BuildEquippedIndexItems(c, c.Inventory.Weapon2, summary);

                int bagIndex = 1;
                foreach (var e in c.Inventory.Sacks)
                {
                    BuildUnequippedIndexItems(c, e.Items.ToArray(), summary, "Bag " + (bagIndex++));
                }

                bagIndex = 1;
                foreach (var e in c.Stash.stashPages)
                {
                    BuildUnequippedIndexItems(c, e.items.ToArray(), summary, "Stash " + (bagIndex++));
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

                var item = BuildIndexItem(e, c);
                if (item != null)
                {
                    item.Bag = "Equipped";
                    item.IsEquipped = true;
                }

                AddIndexItem(item, summary);
            }
        }

        private void BuildUnequippedIndexItems(CharacterFile c, Item[] items, IndexSummary summary, string bagName)
        {
            foreach (var e in items)
            {
                if (e == null)
                    continue;

                var item = BuildIndexItem(e, c);
                if (item != null)
                    item.Bag = bagName;

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

        private IndexItem BuildIndexItem(Item item, CharacterFile character)
        {
            if (string.IsNullOrEmpty(item.baseName))
                return null;

            var itemDef = _itemCache.GetItem(item.baseName);

            if (itemDef == null)
                return null;

            var indexItem = new IndexItem();
            indexItem.ItemName = ItemHelper.GetFullItemName(item, itemDef);
            indexItem.Owner = character.Header.Name;
            if (itemDef.NumericalParametersRaw.ContainsKey("levelRequirement"))
                indexItem.LevelRequirement = (int)itemDef.NumericalParametersRaw["levelRequirement"];

            indexItem.Rarity = ItemHelper.GetItemRarity(itemDef);
            indexItem.ItemType = ItemHelper.GetItemType(itemDef);
            indexItem.Source = itemDef;
            indexItem.SourceInstance = item;
            indexItem.ItemStats = ItemHelper.GetStats(item, itemDef).Select(x=>x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "")).ToList();
            indexItem.Searchable = BuildSearchableString(character, item, itemDef, indexItem.ItemStats);

            return indexItem;
        }

        private bool FilterMatch(IndexItem item, IndexFilter filter)
        {
            if (filter.MaxLevel != null && item.LevelRequirement > filter.MaxLevel)
                return false;

            if (filter.MinLevel != null && item.LevelRequirement < filter.MinLevel)
                return false;

            if (filter.IncludeEquipped != null && (!filter.IncludeEquipped.Value && item.IsEquipped))
                return false;

            if (filter.ItemQualities != null && !filter.ItemQualities.Contains(item.Rarity))
                return false;

            if (filter.ItemTypes != null && !filter.ItemTypes.Contains(item.ItemType))
                return false;

            return true;
        }

        private string BuildSearchableString(CharacterFile character, Item item, ItemRaw itemDef, List<string> itemStats)
        {
            List<string> searchableStrings = new List<string>();

            searchableStrings.Add(ItemHelper.GetFullItemName(item, itemDef).ToLower());
            searchableStrings.AddRange(itemStats);
            searchableStrings.Add(character.Header.Name);

            return string.Join(" ", searchableStrings).ToLower();
        }
    }
}
