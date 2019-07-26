using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public class ItemCacheContainer
    {
        public string GrimDawnLastUpdated { get; set; }
        public string Version { get; set; }

        public Dictionary<string, ItemRaw> Items = new Dictionary<string, ItemRaw>();
    }
}
