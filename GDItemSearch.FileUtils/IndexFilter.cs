using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils
{
    public class IndexFilter
    {
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }

        public bool? IncludeEquipped { get; set; }

        public int? PageSize { get; set; }

        public string[] ItemTypes { get; set; }

        public string[] ItemQualities { get; set; }

        public string SearchMode { get; set; }
    }
}
