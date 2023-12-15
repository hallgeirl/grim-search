using GrimSearch.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils
{
    public class SearchResult
    {
        public SearchResult()
        {
            Results = new List<IndexItem>();
        }

        public SearchResult(List<IndexItem> results)
        {
            Results = results;
            TotalCount = results.Count;
        }


        public SearchResult(List<IndexItem> results, int totalCount)
        {
            Results = results;
            TotalCount = totalCount;
        }

        public List<IndexItem> Results { get; set; }
        public int TotalCount { get; set; }
    }
}
