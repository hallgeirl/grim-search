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
using System.Threading.Tasks;


namespace GrimSearch.Utils
{
    public interface IIndex
    {
        Task<SearchResult> FindAsync(string search, IndexFilter filter);
        Task<SearchResult> FindDuplicatesAsync(string search, IndexFilter filter);
        Task<SearchResult> FindUniqueAsync(string search, IndexFilter filter);
        void ClearCache();
        Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck);
        Task<IndexSummary> BuildAsync(string grimDawnDirectory, string grimDawnSavesDirectory, bool keepExtractedFiles, bool skipVersionCheck, Action<string> stateChangeCallback);
    }
}
