using GrimSearch.Utils;
using GrimSearch.Utils.DBFiles;

namespace GrimSearch.ViewModels
{
    public class ItemViewModel
    {
        public string Name { get; set; }
        public int LevelRequirement { get; set; }
        public string Owner { get; set; }
        public string CoreStats { get; set; }
        public string Bag { get; set; }
        public string DuplicatesOn { get; set; }

        public string ItemColor
        {
            get; set;
        }
        public static ItemViewModel FromModel(IndexItem item)
        {
            string itemColor = "Black";
            switch (item.Rarity)
            {
                case "Legendary":
                    itemColor = "Purple";
                    break;
                case "Rare":
                    itemColor = "Green";
                    break;
                case "Magic":
                    itemColor = "Orange";
                    break;
                case "Epic":
                    itemColor = "Blue";
                    break;
                default:
                    break;

            }
            return new ItemViewModel()
            {
                Name = ItemHelper.GetFullItemName(item.SourceInstance, item.Source),
                LevelRequirement = item.LevelRequirement,
                Owner = item.Owner,
                CoreStats = string.Join(", ", item.ItemStats),
                ItemColor = itemColor,
                Bag = item.Bag,
                DuplicatesOn = item.DuplicatesOnCharacters != null ? string.Join(", ", item.DuplicatesOnCharacters) : ""
            };
        }
    }
}
