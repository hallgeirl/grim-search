using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GrimSearch.Utils;
using GrimSearch.Utils.CharacterFiles;
using GrimSearch.Utils.DBFiles;

public abstract class IndexBase : IIndex
{
    protected ItemCache _itemCache = ItemCache.Instance;
    protected StringsCache _stringsCache = StringsCache.Instance;

    private string _formulasFilename;
    public IndexBase(string formulasFilename)
    {
        _formulasFilename = formulasFilename;
    }

    public async Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck)
    {
        return await Task.Run(() => Build(grimDawnDirectory, grimDawnSavesDirectory, keepExtractedFiles, skipVersionCheck, (msg) => { })).ConfigureAwait(false);
    }

    public async Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback)
    {
        return await Task.Run(() => Build(grimDawnDirectory, grimDawnSavesDirectory, keepExtractedFiles, skipVersionCheck, stateChangeCallback)).ConfigureAwait(false);
    }

    public async Task<SearchResult> FindAsync(string search, IndexFilter filter)
    {
        return await Task.Run(() => Find(search, filter)).ConfigureAwait(false);
    }

    public async Task<SearchResult> FindDuplicatesAsync(string search, IndexFilter filter)
    {
        return await Task.Run(() => FindDuplicates(search, filter)).ConfigureAwait(false);
    }

    public async Task<SearchResult> FindUniqueAsync(string search, IndexFilter filter)
    {
        return await Task.Run(() => FindUnique(search, filter)).ConfigureAwait(false);
    }

    protected abstract IndexSummary BuildIndex(List<CharacterFile> characters, Action<string> stateChangeCallback);

    protected abstract SearchResult Find(string search, IndexFilter filter);

    protected abstract SearchResult FindDuplicates(string search, IndexFilter filter);

    protected abstract SearchResult FindUnique(string search, IndexFilter filter);

    private IndexSummary Build(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback)
    {
        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var characters = CharacterLoader.LoadAllCharacters(grimDawnSavesDirectory, stateChangeCallback, _formulasFilename);

            if (StringsCache.Instance.IsDirty)
            {
                stateChangeCallback("Loading tags/strings");
                StringsCache.Instance.LoadAllStrings(grimDawnDirectory);
            }

            if (ItemCache.Instance.IsDirty)
            {
                stateChangeCallback("Loading items");
                ItemCache.Instance.LoadAllItems(grimDawnDirectory, keepExtractedFiles, skipVersionCheck, stateChangeCallback);
            }

            var summary = BuildIndex(characters, stateChangeCallback);

            MD5Store.Instance.Save(ConfigFileHelper.GetConfigFile("DatabaseHashes.json"));

            return summary;
        }
        finally
        {
            sw.Stop();
            Metrics.IndexBuildTime.Record(sw.ElapsedMilliseconds);
        }
    }

}