using GDItemSearch.Utils.CharacterFiles;
using GDItemSearch.Utils.DBFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils
{
    public class IndexItem
    {
        public string ItemName { get; set; }
        public bool IsEquipped { get; set; }

        public int LevelRequirement { get; set; }

        public string Owner { get; set; }

        public string Searchable { get; set; }
        
        public string ItemType { get; set; }

        public string Rarity { get; set; }

        public string Bag { get; set; }

        public List<string> DuplicatesOnCharacters { get; set; }

        public List<string> ItemStats { get; set; }

        public ItemRaw Source { get; set; }
        public Item SourceInstance { get; set; }
    }
}
