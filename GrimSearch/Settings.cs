using GrimSearch.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch
{
    public class StoredSettings
    {
        public StoredSettings()
        {
        }

        public string GrimDawnDirectory { get; set; }
        public string SavesDirectory { get; set; }
        public string SearchEngine { get; set; }
        public bool AutoRefresh { get; set; }

        public string LastSearchMode { get; set; }
        public string LastSearchText { get; set; }
        public bool KeepExtractedDBFiles { get; set; }
    }
}
