using GDItemSearch.FileUtils.CharacterFiles;
using GDItemSearch.FileUtils.DBFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils
{
    public class IndexItem
    {
        public bool IsEquipped { get; set; }

        public int LevelRequirement { get; set; }

        public string Owner { get; set; }

        public string Searchable { get; set; }
        
        public string ItemType { get; set; }

        public string Rarity { get; set; }

        public ItemRaw Source { get; set; }
        public Item SourceInstance { get; set; }
    }
}
