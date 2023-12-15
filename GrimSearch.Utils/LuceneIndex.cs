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
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace GrimSearch.Utils
{
    public class LuceneIndex : IIndex, IDisposable
    {
        Lucene.Net.Store.Directory _indexRamDir;
        IndexReader _indexReader;

        string formulasFilename = "formulas.gst";

        public LuceneIndex(string formulasFilename = "formulas.gst")
        {
            this.formulasFilename = formulasFilename;
        }

        public LuceneIndex(ItemCache itemCache, StringsCache stringsCache, string formulasFilename = "formulas.gst") : this(formulasFilename)
        {
            _itemCache = itemCache;
            _stringsCache = stringsCache;
        }

        string[] _reservedCharacters = new string[]{
            "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", "*", "?", ":", "'"
        };
        ItemCache _itemCache = ItemCache.Instance;
        StringsCache _stringsCache = StringsCache.Instance;
        List<CharacterFile> _characters = new List<CharacterFile>();

        public async Task<SearchResult> FindAsync(string search, IndexFilter filter)
        {
            return await Task.Run(() => Find(search, filter)).ConfigureAwait(false);
        }

        private SearchResult Find(string search, IndexFilter filter)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                search = search ?? "";
                search = SanitizeSearchString(search);

                var searcher = new IndexSearcher(_indexReader);
                var searchTerms = search.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x));

                var fullQuery = new BooleanQuery();

                foreach (var searchTerm in searchTerms)
                {
                    var query = new WildcardQuery(new Term("searchable", "*" + searchTerm.ToLowerInvariant() + "*"));
                    fullQuery.Add(query, Occur.MUST);
                }

                AddFilterQueries(fullQuery, filter);

                TopDocs topDocs = searcher.Search(fullQuery, n: 100);

                List<IndexItem> results = new List<IndexItem>();

                foreach (var sdoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(sdoc.Doc);
                    results.Add(DocumentToIndexItem(doc));
                }

                return new SearchResult(results, topDocs.TotalHits);
            }
            finally
            {
                sw.Stop();
                Metrics.SearchTime.Record(sw.ElapsedMilliseconds);
            }
        }

        private string SanitizeSearchString(string searchString)
        {
            foreach (var c in _reservedCharacters)
            {
                searchString = searchString.Replace(c, "");
            }
            return searchString;
        }

        public async Task<SearchResult> FindDuplicatesAsync(string search, IndexFilter filter)
        {
            return await Task.Run(() => FindDuplicates(search, filter)).ConfigureAwait(false);
        }


        private SearchResult FindDuplicates(string search, IndexFilter filter)
        {
            return new SearchResult();
            /*var sw = new Stopwatch();
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
            }*/
        }


        public async Task<SearchResult> FindUniqueAsync(string search, IndexFilter filter)
        {
            return await Task.Run(() => FindUnique(search, filter)).ConfigureAwait(false);
        }

        private SearchResult FindUnique(string search, IndexFilter filter)
        {
            return new SearchResult();
            /*var sw = new Stopwatch();
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
            }*/
        }

        public void ClearCache()
        {
            _itemCache.ClearCache();
            _stringsCache.ClearCache();
        }

        public async Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck)
        {
            return await Task.Run(() => Build(grimDawnDirectory, grimDawnSavesDirectory, keepExtractedFiles, skipVersionCheck, (msg) => { })).ConfigureAwait(false);
        }

        public async Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback)
        {
            return await Task.Run(() => Build(grimDawnDirectory, grimDawnSavesDirectory, keepExtractedFiles, skipVersionCheck, stateChangeCallback)).ConfigureAwait(false);
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
            indexItem.ItemType = doc.GetField("itemType").GetStringValue();
            indexItem.Searchable = doc.GetField("searchable").GetStringValue();
            indexItem.ItemStats = new List<string>();
            return indexItem;
            /*
            //indexItem.Source = itemDef;
            //indexItem.SourceInstance = item;
            var itemStats = ItemHelper.GetStats(item, itemStatDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList(); ;
            var itemPetStats = petStatItemDef != null ? ItemHelper.GetStats(null, petStatItemDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList() : new List<string>();
            //indexItem.ItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();
            var allItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();
            indexItem.AddTextField("searchable", BuildSearchableString(character, item, itemDef, allItemStats), Field.Store.YES);

            UpdateSummary(rarity, itemType, character.Header.Name, summary);
*/

        }

        private IndexSummary Build(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                LoadAllCharacters(grimDawnSavesDirectory, stateChangeCallback);

                stateChangeCallback("Loading tags/strings");
                _stringsCache.LoadAllStrings(grimDawnDirectory);

                stateChangeCallback("Loading items");
                _itemCache.LoadAllItems(grimDawnDirectory, keepExtractedFiles, skipVersionCheck, stateChangeCallback);

                var summary = BuildIndex(stateChangeCallback);

                MD5Store.Instance.Save(ConfigFileHelper.GetConfigFile("DatabaseHashes.json"));

                return summary;
            }
            finally
            {
                sw.Stop();
                Metrics.IndexBuildTime.Record(sw.ElapsedMilliseconds);
            }
        }

        private void LoadAllCharacters(string grimDawnSavesDirectory, Action<string> stateChangeCallback)
        {
            stateChangeCallback("Clearing index");
            _characters.Clear();

            var charactersDirectory = Path.Combine(grimDawnSavesDirectory, "main");
            if (!System.IO.Directory.Exists(charactersDirectory))
                throw new InvalidOperationException("Saves directory not found: " + charactersDirectory);

            var directories = System.IO.Directory.EnumerateDirectories(charactersDirectory, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x);

            foreach (var d in directories)
            {
                //Skip backup characters
                if (Path.GetFileName(d).StartsWith("__"))
                    continue;

                var characterFile = Path.Combine(d, "player.gdc");
                if (!File.Exists(characterFile))
                    continue;

                stateChangeCallback("Loading " + characterFile);

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

            LoadTransferStashAsCharacter(grimDawnSavesDirectory, stateChangeCallback);
            LoadBlueprintsAsCharacter(grimDawnSavesDirectory, stateChangeCallback);
        }

        private void LoadTransferStashAsCharacter(string grimDawnSavesDirectory, Action<string> stateChangeCallback)
        {
            var transferStashFile = Path.Combine(grimDawnSavesDirectory, "transfer.gst");
            stateChangeCallback("Loading " + transferStashFile);
            var transferStash = new TransferStashFile();
            using (var s = File.OpenRead(transferStashFile))
            {
                transferStash.Read(s);
            }

            _characters.Add(transferStash.ToCharacterFile());
        }

        private void LoadBlueprintsAsCharacter(string grimDawnSavesDirectory, Action<string> stateChangeCallback)
        {
            var recipesFilePath = Path.Combine(grimDawnSavesDirectory, formulasFilename);
            stateChangeCallback("Loading " + recipesFilePath);
            var recipes = new BlueprintFile();
            using (var s = File.OpenRead(recipesFilePath))
            {
                recipes.Read(s);
            }

            _characters.Add(recipes.ToCharacterFile());
        }

        private IndexSummary BuildIndex(Action<string> stateChangeCallback)
        {
            _indexRamDir?.Dispose();
            _indexReader?.Dispose();

            _indexRamDir = new RAMDirectory();
            using var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48, new Lucene.Net.Analysis.Util.CharArraySet(LuceneVersion.LUCENE_48, 0, true));

            var idxCfg = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            idxCfg.OpenMode = OpenMode.CREATE;

            using var writer = new IndexWriter(_indexRamDir, idxCfg);


            var summary = new IndexSummary();

            foreach (var c in _characters)
            {
                stateChangeCallback("Indexing " + c.Header.Name);
                BuildEquippedIndexItems(writer, c, c.Inventory.Equipment, summary);
                BuildEquippedIndexItems(writer, c, c.Inventory.Weapon1, summary);
                BuildEquippedIndexItems(writer, c, c.Inventory.Weapon2, summary);

                int bagIndex = 1;
                foreach (var e in c.Inventory.Sacks)
                {
                    BuildUnequippedIndexItems(writer, c, e.Items.ToArray(), summary, "Bag " + (bagIndex++));
                }

                bagIndex = 1;
                foreach (var e in c.Stash.stashPages)
                {
                    BuildUnequippedIndexItems(writer, c, e.items.ToArray(), summary, "Stash " + (bagIndex++));
                }
            }
            writer.Commit();
            _indexReader = writer.GetReader(true);

            return summary;
        }

        private void BuildEquippedIndexItems(IndexWriter writer, CharacterFile c, Item[] equipment, IndexSummary summary)
        {
            foreach (var e in equipment)
            {
                if (e == null)
                    continue;

                var item = BuildIndexItem(e, c, summary);
                if (item != null)
                {
                    item.AddStringField("bag", "Equipped", Field.Store.YES);
                    item.AddInt32Field("isEquipped", 1, Field.Store.YES);
                    writer.AddDocument(item);
                }
            }
        }

        private void BuildUnequippedIndexItems(IndexWriter writer, CharacterFile c, Item[] items, IndexSummary summary, string bagName)
        {
            foreach (var e in items)
            {
                if (e == null)
                    continue;

                var item = BuildIndexItem(e, c, summary);

                if (item != null)
                {
                    item.AddStringField("bag", bagName, Field.Store.YES);
                    item.AddInt32Field("isEquipped", 0, Field.Store.YES);
                    writer.AddDocument(item);
                }
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

        private Document BuildIndexItem(Item item, CharacterFile character, IndexSummary summary)
        {
            if (string.IsNullOrEmpty(item.baseName))
                return null;

            var itemDef = _itemCache.GetItem(item.baseName);
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
            indexItem.AddTextField("itemName", ItemHelper.GetFullItemName(item, itemDef), Field.Store.YES);
            indexItem.AddTextField("owner", character.Header.Name, Field.Store.YES);
            if (itemStatDef.NumericalParametersRaw.ContainsKey("levelRequirement"))
                indexItem.Add(new Int32Field("levelRequirement", (int)itemStatDef.NumericalParametersRaw["levelRequirement"], Field.Store.YES));

            if (rarity != null)
                indexItem.AddStringField("rarity", rarity, Field.Store.YES);

            indexItem.AddStringField("itemType", ItemHelper.GetItemType(itemStatDef), Field.Store.YES);
            //indexItem.Source = itemDef;
            //indexItem.SourceInstance = item;
            var itemStats = ItemHelper.GetStats(item, itemStatDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList(); ;
            var itemPetStats = petStatItemDef != null ? ItemHelper.GetStats(null, petStatItemDef).Select(x => x.Replace("{^E}", "").Replace("{%+.0f0}", "").Replace("{%t0}", "").Trim()).ToList() : new List<string>();
            //indexItem.ItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();
            var allItemStats = itemStats.Union(itemPetStats.Select(x => $"{x} to pets")).ToList();
            //indexItem.AddTextField("searchable", BuildSearchableString(character, item, itemDef, allItemStats), Field.Store.YES);
            foreach (var i in GetSearchableStrings(character, item, itemDef, allItemStats))
            {
                indexItem.AddTextField("searchable", i, Field.Store.YES);
            }

            UpdateSummary(rarity, itemType, character.Header.Name, summary);

            return indexItem;
        }

        private void AddFilterQueries(BooleanQuery fullQuery, IndexFilter filter)
        {
            if (filter.MaxLevel != null)
                fullQuery.Add(NumericRangeQuery.NewInt32Range("levelRequirement", null, filter.MaxLevel, true, true), Occur.MUST);

            if (filter.MinLevel != null)
                fullQuery.Add(NumericRangeQuery.NewInt32Range("levelRequirement", filter.MinLevel, null, true, true), Occur.MUST);

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

        private string BuildSearchableString(CharacterFile character, Item item, ItemRaw itemDef, List<string> itemStats)
        {
            List<string> searchableStrings = new List<string>();

            searchableStrings.Add(ItemHelper.GetFullItemName(item, itemDef).ToLower());
            searchableStrings.AddRange(itemStats);
            searchableStrings.Add(character.Header.Name);

            return string.Join(" ", searchableStrings).ToLower();
        }

        private List<string> GetSearchableStrings(CharacterFile character, Item item, ItemRaw itemDef, List<string> itemStats)
        {
            List<string> searchableStrings = new List<string>();

            searchableStrings.AddRange(SanitizeSearchString(ItemHelper.GetFullItemName(item, itemDef)).Split(" "));
            searchableStrings.AddRange(itemStats.Select(x => SanitizeSearchString(x)));
            searchableStrings.Add(SanitizeSearchString(character.Header.Name));

            return searchableStrings;
        }

        public void Dispose()
        {
            _indexRamDir?.Dispose();
            _indexReader?.Dispose();
        }
    }
}
