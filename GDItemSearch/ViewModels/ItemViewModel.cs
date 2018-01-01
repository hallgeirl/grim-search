using GDItemSearch.FileUtils;
using GDItemSearch.FileUtils.DBFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.ViewModels
{
    public class ItemViewModel
    {
        public string Name { get; set; }
        public int LevelRequirement { get; set; }
        public string Owner { get; set; }
        public string CoreStats { get; set; }
        public static ItemViewModel FromModel(IndexItem item)
        {
            return new ItemViewModel()
            {
                Name = ItemHelper.GetFullItemName(item.SourceInstance, item.Source),
                LevelRequirement = item.LevelRequirement,
                Owner = item.Owner,
                CoreStats = string.Join(", ", item.ItemStats)
            };
        }
    }
}
