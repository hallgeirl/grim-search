using GrimSearch.Utils.CharacterFiles;
using GrimSearch.Utils.DBFiles;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Codecs;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace GrimSearch.Utils
{
    public class LuceneIndex : IndexBase, IDisposable
    {
        Lucene.Net.Store.Directory _indexRamDir;
        IndexReader _indexReader;

        public LuceneIndex(string formulasFilename = "formulas.gst") : base(formulasFilename)
        {
        }

        public LuceneIndex(ItemCache itemCache, StringsCache stringsCache, string formulasFilename = "formulas.gst") : this(formulasFilename)
        {
            _itemCache = itemCache;
            _stringsCache = stringsCache;
        }

        string[] _reservedCharacters = new string[]{
            "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", "*", "?", ":", "'"
        };

        IndexSearcher _searcher;

        #region Public methods

        public void Dispose()
        {
            _indexRamDir?.Dispose();
            _indexReader?.Dispose();
        }

        #endregion

        #region Protected methods
        protected override SearchResult Find(string search, IndexFilter filter)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                search = SanitizeSearchString(search);

                var searchTerms = search.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x));

                var fullQuery = new BooleanQuery();

                foreach (var searchTerm in searchTerms)
                {
                    var query = new WildcardQuery(new Term("searchable", searchTerm.ToLowerInvariant() + "*"));
                    fullQuery.Add(query, Occur.MUST);
                }

                AddFilterQueries(fullQuery, filter);

                return SearchAndGetTopResults(fullQuery);
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
                SanitizeSearchString(search);

                var fullQuery = new BooleanQuery();
                fullQuery.Add(new TermQuery(new Term("owner", search.ToLowerInvariant())), Occur.MUST);
                fullQuery.Add(new TermQuery(new Term("duplicatesOn", "")), Occur.MUST_NOT);
                AddFilterQueries(fullQuery, filter);

                return SearchAndGetTopResults(fullQuery);
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
                SanitizeSearchString(search);

                var fullQuery = new BooleanQuery();
                fullQuery.Add(new TermQuery(new Term("owner", search.ToLowerInvariant())), Occur.MUST);
                fullQuery.Add(new TermQuery(new Term("duplicatesOn", "")), Occur.MUST);
                AddFilterQueries(fullQuery, filter);

                return SearchAndGetTopResults(fullQuery);
            }
            finally
            {
                sw.Stop();
                Metrics.SearchTime.Record(sw.ElapsedMilliseconds);
            }
        }

        protected override IndexSummary BuildIndex(List<CharacterFile> characters, Action<string> stateChangeCallback)
        {
            _indexRamDir?.Dispose();
            _indexReader?.Dispose();

            var itemsByName = GroupItemsByName(characters);

            _indexRamDir = new RAMDirectory();
            using var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48, new Lucene.Net.Analysis.Util.CharArraySet(LuceneVersion.LUCENE_48, 0, true));

            var idxCfg = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            idxCfg.OpenMode = OpenMode.CREATE;

            using var writer = new IndexWriter(_indexRamDir, idxCfg);


            var summary = new IndexSummary();

            stateChangeCallback("Adding items to the index");
            foreach (var itemName in itemsByName.Keys)
            {
                foreach (var item in itemsByName[itemName])
                {

                    AddItemToIndex(writer, item, itemsByName, summary);
                }
            }
            writer.Commit();
            _indexReader = writer.GetReader(true);
            _searcher = new IndexSearcher(_indexReader);

            return summary;
        }

        #endregion

        #region Private methods

        private SearchResult SearchAndGetTopResults(BooleanQuery fullQuery)
        {
            TopDocs topDocs = _searcher.Search(fullQuery, n: 1000000);

            List<IndexItem> results = new List<IndexItem>();

            foreach (var sdoc in topDocs.ScoreDocs)
            {
                Document doc = _searcher.Doc(sdoc.Doc);
                results.Add(DocumentToIndexItem(doc));
            }

            return new SearchResult(results, topDocs.TotalHits);
        }

        private string SanitizeSearchString(string searchString)
        {
            searchString = searchString ?? "";
            foreach (var c in _reservedCharacters)
            {
                searchString = searchString.Replace(c, "");
            }
            return searchString;
        }

        private IndexItem DocumentToIndexItem(Document doc)
        {
            var indexItem = new IndexItem();
            indexItem.ItemName = doc.GetField("itemName").GetStringValue();
            indexItem.Owner = doc.GetField("owner").GetStringValue();
            indexItem.Rarity = doc.GetField("rarity")?.GetStringValue();
            var levelRequirement = doc.GetField("levelRequirement")?.GetInt32Value();
            if (levelRequirement != null)
                indexItem.LevelRequirement = levelRequirement.Value;

            indexItem.ItemType = doc.GetField("itemType").GetStringValue();
            indexItem.ItemStats = new List<string>();
            indexItem.DuplicatesOnCharacters = doc.GetField("duplicatesOn").GetStringValue()?.Split(",").ToList();
            indexItem.Bag = doc.GetField("bag").GetStringValue();
            indexItem.ItemStats = doc.GetField("itemStats").GetStringValue().Split(",").ToList();

            return indexItem;
        }

        public struct ItemWrapper
        {
            public string ItemName;
            public Item item;
            public ItemRaw itemDef;
            public bool IsEquipped;
            public string BagName;
            public string CharacterName;
        }

        // Group items by name.
        // Returns a tuple of:
        // - Item
        // - Is equipped - true/false
        // - Bag name
        private Dictionary<string, List<ItemWrapper>> GroupItemsByName(List<CharacterFile> characters)
        {
            var result = new Dictionary<string, List<ItemWrapper>>();

            foreach (var c in characters)
            {
                AddItemsToItemGroup(c.Inventory.Equipment, c, "Equipped", true, result);
                AddItemsToItemGroup(c.Inventory.Weapon1, c, "Equipped", true, result);
                AddItemsToItemGroup(c.Inventory.Weapon2, c, "Equipped", true, result);

                int bagIndex = 1;
                foreach (var e in c.Inventory.Sacks)
                {
                    AddItemsToItemGroup(e.Items.ToArray(), c, "Bag " + (bagIndex++), false, result);
                }

                bagIndex = 1;
                foreach (var e in c.Stash.stashPages)
                {
                    AddItemsToItemGroup(e.items.ToArray(), c, "Stash " + (bagIndex++), false, result);
                }
            }
            return result;
        }

        private void AddItemsToItemGroup(Item[] items, CharacterFile character, string bagName, bool isEquipped, Dictionary<string, List<ItemWrapper>> result)
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var itemDef = _itemCache.GetItem(item.baseName);
                if (itemDef == null)
                    continue;

                var itemName = ItemHelper.GetFullItemName(item, itemDef);
                var itemWrapper = new ItemWrapper()
                {
                    ItemName = itemName,
                    item = item,
                    itemDef = itemDef,
                    IsEquipped = isEquipped,
                    BagName = bagName,
                    CharacterName = character.Header.Name
                };

                if (!result.ContainsKey(itemName))
                {
                    result.Add(itemName, new List<ItemWrapper>());
                }
                result[itemName].Add(itemWrapper);
            }
        }



        private void AddItemToIndex(IndexWriter writer, ItemWrapper itemWrapper, Dictionary<string, List<ItemWrapper>> itemsByName, IndexSummary summary)
        {
            var item = BuildIndexItem(itemWrapper, itemsByName, summary);
            if (item != null)
            {
                item.AddStringField("bag", itemWrapper.BagName, Field.Store.YES);
                item.AddInt32Field("isEquipped", itemWrapper.IsEquipped ? 1 : 0, Field.Store.YES);
                writer.AddDocument(item);
            }
        }
        private void UpdateSummary(string rarity, string itemType, string owner, IndexSummary summary)
        {
            summary.Entries++;

            if (rarity != null)
                summary.ItemRarities.Add(rarity);

            if (itemType != null)
                summary.ItemTypes.Add(itemType);

            if (owner != null)
                summary.Characters.Add(owner);
        }

        SortedSet<string> _unwantedItemTypes = new SortedSet<string>() {
            "OneShot_PotionMana",
            "OneShot_PotionHealth",
            "OneShot_Scroll",
            "ItemEnchantment",
            "QuestItem",
            "ItemTransmuter",
            "ItemNote"
        };

        private Document BuildIndexItem(ItemWrapper itemWrapper, Dictionary<string, List<ItemWrapper>> itemsByName, IndexSummary summary)
        {
            if (string.IsNullOrEmpty(itemWrapper.item.baseName))
                return null;

            var itemDef = itemWrapper.itemDef;
            if (itemDef == null)
                return null;

            // Filter out unwanted item types
            var itemType = ItemHelper.GetItemType(itemDef);
            if (_unwantedItemTypes.Contains(itemType))
            {
                return null;
            }

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

            var rarity = ItemHelper.GetItemRarity(itemDef);

            var indexItem = new Document();
            indexItem.AddTextField("itemName", itemWrapper.ItemName, Field.Store.YES);
            indexItem.AddTextField("owner", itemWrapper.CharacterName, Field.Store.YES);
            if (itemStatDef.NumericalParametersRaw.ContainsKey("levelRequirement"))
                indexItem.Add(new Int32Field("levelRequirement", (int)itemStatDef.NumericalParametersRaw["levelRequirement"], Field.Store.YES));
            else
                indexItem.Add(new Int32Field("levelRequirement", 0, Field.Store.YES));

            if (rarity != null)
                indexItem.AddStringField("rarity", rarity, Field.Store.YES);

            indexItem.AddStringField("itemType", ItemHelper.GetItemType(itemStatDef), Field.Store.YES);
            var itemStats = ItemHelper.GetStats(itemWrapper.item, itemStatDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList(); ;
            var itemPetStats = petStatItemDef != null ? ItemHelper.GetStats(null, petStatItemDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList() : new List<string>();
            var allItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();
            indexItem.AddStringField("itemStats", string.Join(",", allItemStats), Field.Store.YES);

            var duplicates = itemsByName[itemWrapper.ItemName].Where(x => x.CharacterName != itemWrapper.CharacterName);
            indexItem.AddStringField("duplicatesOn", string.Join(",", duplicates.Select(x => x.CharacterName)), Field.Store.YES);
            foreach (var i in GetSearchableStrings(itemWrapper, allItemStats))
            {
                indexItem.AddTextField("searchable", i, Field.Store.NO);
            }

            UpdateSummary(rarity, itemType, itemWrapper.CharacterName, summary);

            return indexItem;
        }

        private void AddFilterQueries(BooleanQuery fullQuery, IndexFilter filter)
        {
            if (filter.MaxLevel != null || filter.MinLevel != null)
                fullQuery.Add(NumericRangeQuery.NewInt32Range("levelRequirement", filter.MinLevel, filter.MaxLevel, true, true), Occur.MUST);

            if (filter.IncludeEquipped != null && filter.IncludeEquipped.Value)
                fullQuery.Add(NumericRangeQuery.NewInt32Range("isEquipped", 0, 1, true, true), Occur.MUST);
            else
                fullQuery.Add(NumericRangeQuery.NewInt32Range("isEquipped", 0, 0, true, true), Occur.MUST);

            if (filter.ItemQualities != null)
            {
                var qualitiesQuery = new BooleanQuery();
                foreach (var itemQuality in filter.ItemQualities)
                {
                    qualitiesQuery.Add(new TermQuery(new Term("rarity", itemQuality)), Occur.SHOULD);
                }
                fullQuery.Add(qualitiesQuery, Occur.MUST);
            }

            if (filter.ItemTypes != null)
            {
                var itemTypesQuery = new BooleanQuery();
                foreach (var itemType in filter.ItemTypes)
                {
                    itemTypesQuery.Add(new TermQuery(new Term("itemType", itemType)), Occur.SHOULD);
                }
                fullQuery.Add(itemTypesQuery, Occur.MUST);
            }
        }

        private List<string> GetSearchableStrings(ItemWrapper itemWrapper, List<string> itemStats)
        {
            List<string> searchableStrings = new List<string>();

            searchableStrings.AddRange(SanitizeSearchString(ItemHelper.GetFullItemName(itemWrapper.item, itemWrapper.itemDef)).Split(" "));
            searchableStrings.AddRange(itemStats.Select(x => SanitizeSearchString(x)));
            searchableStrings.Add(SanitizeSearchString(itemWrapper.CharacterName));

            return searchableStrings;
        }
        #endregion
    }
}
