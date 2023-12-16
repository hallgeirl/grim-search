using GrimSearch.Utils.CharacterFiles;
using GrimSearch.Utils.DBFiles;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace GrimSearch.Utils
{
    public class Index : IndexBase
    {
        public Index(string formulasFilename = "formulas.gst") : base(formulasFilename)
        {
        }

        public Index(ItemCache itemCache, StringsCache stringsCache, string formulasFilename = "formulas.gst") : this(formulasFilename)
        {
            _itemCache = itemCache;
            _stringsCache = stringsCache;
        }

        List<IndexItem> _index = new List<IndexItem>(); //Not really an index though.. for now ;)

        #region Protected methods
        protected override SearchResult Find(string search, IndexFilter filter)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                search = search ?? "";

                var result = _index.Where(x => x.Searchable.Contains(search.ToLower()) && FilterMatch(x, filter));

                if (filter.PageSize != null)
                    return new SearchResult(result.Take(filter.PageSize.Value).ToList(), result.Count());

                return new SearchResult(result.ToList());
            }
            finally
            {
                sw.Stop();
                Metrics.SearchTime.Record(sw.ElapsedMilliseconds);
            }
        }

        protected override SearchResult FindDuplicates(string search, IndexFilter filter)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                search = search ?? "";

                // Try searching for partial string first -- if this gives an unique result, go with it
                var characterItems = _index.Where(x => x.Owner.ToLower().Contains(search.ToLower()) && FilterMatch(x, filter));

                // Otherwise, match the full character name
                if (characterItems.Select(x => x.Owner.ToLower()).Distinct().Count() > 1)
                    characterItems = _index.Where(x => x.Owner.ToLower() == search.ToLower() && FilterMatch(x, filter));

                search = characterItems.FirstOrDefault()?.Owner ?? search;

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
                return new SearchResult(results.OrderBy(x => x.Bag).ToList());
            }
            finally
            {
                sw.Stop();
                Metrics.SearchTime.Record(sw.ElapsedMilliseconds);
            }
        }

        protected override SearchResult FindUnique(string search, IndexFilter filter)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                search = search ?? "";

                // Try searching for partial string first -- if this gives an unique result, go with it
                var characterItems = _index.Where(x => x.Owner.ToLower().Contains(search.ToLower()) && FilterMatch(x, filter));

                // Otherwise, match the full character name
                if (characterItems.Select(x => x.Owner.ToLower()).Distinct().Count() > 1)
                    characterItems = _index.Where(x => x.Owner.ToLower() == search.ToLower() && FilterMatch(x, filter));

                search = characterItems.FirstOrDefault()?.Owner ?? search;

                List<IndexItem> results = new List<IndexItem>();

                foreach (var item in characterItems)
                {
                    var itemName = ItemHelper.GetFullItemName(item.SourceInstance, item.Source);
                    var dupe = _index.Where(x => x.ItemName.ToLower() == itemName.ToLower() && x.Owner.ToLower() != search.ToLower());

                    if (dupe.Count() == 0)
                    {
                        results.Add(item);
                        item.DuplicatesOnCharacters = dupe.Select(x => x.Owner).ToList();
                    }
                }
                return new SearchResult(results.OrderBy(x => x.Bag).ToList());
            }
            finally
            {
                sw.Stop();
                Metrics.SearchTime.Record(sw.ElapsedMilliseconds);
            }
        }

        protected override IndexSummary BuildIndex(List<CharacterFile> characters, Action<string> stateChangeCallback)
        {
            _index.Clear();
            var summary = new IndexSummary();

            foreach (var c in characters)
            {
                stateChangeCallback("Indexing " + c.Header.Name);
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
        #endregion

        #region Private methods
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

                var rarity = item.Rarity;
                if (rarity != null)
                    summary.ItemRarities.Add(rarity);

                if (itemType != null)
                    summary.ItemTypes.Add(itemType);

                if (item.Owner != null)
                    summary.Characters.Add(item.Owner);

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

            //itemStatDef is the item definition that is used for stats (relevant in case of blueprints, where the item itself doesn't have stats, but the crafted item does)
            var itemStatDefIdentifier = ItemHelper.GetItemStatSource(itemDef);

            ItemRaw itemStatDef = itemDef;
            if (itemStatDefIdentifier != null)
            {
                itemStatDef = _itemCache.GetItem(itemStatDefIdentifier);
                // Some items are defined slightly differently - where the target item actually refers to a loot table instead of the item itself.
                if (itemStatDef != null && itemStatDef.StringParametersRaw.ContainsKey("Class") && itemStatDef.StringParametersRaw["Class"] == "LootItemTable_DynWeight")
                {
                    var lootName1 = itemStatDef.StringParametersRaw.ContainsKey("lootName1") ? itemStatDef.StringParametersRaw["lootName1"] : null;
                    if (lootName1 == null)
                        itemStatDef = null;
                    else
                        itemStatDef = _itemCache.GetItem(lootName1);
                }
            }

            if (itemStatDef == null)
                itemStatDef = itemDef;

            ItemRaw petStatItemDef = null;
            if (itemStatDef.StringParametersRaw.ContainsKey("petBonusName"))
            {
                petStatItemDef = _itemCache.GetItem(itemStatDef.StringParametersRaw["petBonusName"]);
            }

            var indexItem = new IndexItem();
            indexItem.ItemName = ItemHelper.GetFullItemName(item, itemDef);
            indexItem.Owner = character.Header.Name;
            if (itemStatDef.NumericalParametersRaw.ContainsKey("levelRequirement"))
                indexItem.LevelRequirement = (int)itemStatDef.NumericalParametersRaw["levelRequirement"];

            indexItem.Rarity = ItemHelper.GetItemRarity(itemDef);
            indexItem.ItemType = ItemHelper.GetItemType(itemStatDef);
            indexItem.Source = itemDef;
            indexItem.SourceInstance = item;
            var itemStats = ItemHelper.GetStats(item, itemStatDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList(); ;
            var itemPetStats = petStatItemDef != null ? ItemHelper.GetStats(null, petStatItemDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList() : new List<string>();
            indexItem.ItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();

            indexItem.Searchable = BuildSearchableString(character, item, itemDef, indexItem.ItemStats);

            return indexItem;
        }

        private bool FilterMatch(IndexItem item, IndexFilter filter)
        {
            if (filter.MaxLevel != null && item.LevelRequirement > filter.MaxLevel)
                return false;

            if (filter.MinLevel != null && item.LevelRequirement < filter.MinLevel)
                return false;

            if (filter.IncludeEquipped != null && !filter.IncludeEquipped.Value && item.IsEquipped)
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
        #endregion
    }
}
