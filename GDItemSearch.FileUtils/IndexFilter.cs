using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils
{
    public class IndexFilter
    {
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }

        public bool? IsEquipped { get; set; }

        public int? PageSize { get; set; }

        public string[] ItemTypes { get; set; }

        public string[] ItemQualities { get; set; }

        public string SearchMode { get; set; }
    }
}
