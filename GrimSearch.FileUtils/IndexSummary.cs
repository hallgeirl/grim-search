using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils
{
    public class IndexSummary
    {
        public IndexSummary()
        {
            ItemTypes = new HashSet<string>();
            ItemRarities = new HashSet<string>();
            Characters = new HashSet<string>();
        }

        public int Entries { get; set; }
        public HashSet<string> ItemTypes { get; set; }
        public HashSet<string> ItemRarities { get; set; }
        public HashSet<string> Characters { get; set; }
    }
}
