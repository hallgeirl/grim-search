using GDItemSearch.FileUtils.CharacterFiles;
using GDItemSearch.FileUtils.DBFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils
{
    public class Index
    {
        ItemCache _itemCache = new ItemCache();
        List<CharacterFile> _characters = new List<CharacterFile>();
        List<IndexItem> _index = new List<IndexItem>(); //Not really an index though.. for now ;)

        

        public List<IndexItem> Find(string search, IndexFilter filter)
        {
            var result = _index.Where(x => x.Searchable.Contains(search) && FilterMatch(x, filter));

            if (filter.PageSize != null)
                return result.Take(filter.PageSize.Value).ToList();

            return result.ToList();
        }

        public void ClearCache()
        {
            _itemCache.ClearCache();
        }

        public void Build()
        {
            LoadAllCharacters();
            _itemCache.LoadAllItems();

            BuildIndex();
        }

        private void LoadAllCharacters()
        {
            var charactersDirectory = Path.Combine(Settings.SavesDirectory, "main");
            if (!Directory.Exists(charactersDirectory))
                throw new InvalidOperationException("Saves directory not found: " + charactersDirectory);

            var directories = Directory.EnumerateDirectories(charactersDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (var d in directories)
            {
                var characterFile = Path.Combine(d, "player.gdc");
                if (!File.Exists(characterFile))
                    continue;
                var character = new CharacterFile();
                using (var s = File.OpenRead(characterFile))
                {
                    character.Read(s);
                }
                _characters.Add(character);
            }
        }

        private void BuildIndex()
       {
            foreach (var c in _characters)
            {
                BuildEquippedIndexItems(c, c.Inventory.Equipment);
                BuildEquippedIndexItems(c, c.Inventory.Weapon1);
                BuildEquippedIndexItems(c, c.Inventory.Weapon2);

                foreach (var e in c.Inventory.Sacks)
                {
                    foreach (var item in e.Items)
                    {
                        var iitem = BuildIndexItem(item, c);
                        if (iitem != null)
                            _index.Add(iitem);
                    }
                }

                foreach (var e in c.Stash.stashPages)
                {
                    foreach (var item in e.items)
                    {
                        var iitem = BuildIndexItem(item, c);
                        if (iitem != null)
                            _index.Add(iitem);
                    }
                }
            }
        }

        private void BuildEquippedIndexItems(CharacterFile c, InventoryEquipment[] equipment)
        {
            foreach (var e in equipment)
            {
                if (e == null)
                    continue;

                var item = BuildEquippedIndexItem(e, c);
                if (item != null)
                    _index.Add(item);
            }
        }

        private IndexItem BuildEquippedIndexItem(Item item, CharacterFile character)
        {
            var i = BuildIndexItem(item, character);

            if (i == null)
                return null;

            i.IsEquipped = true;
            return i;
        }

        private IndexItem BuildIndexItem(Item item, CharacterFile character)
        {
            if (string.IsNullOrEmpty(item.baseName))
                return null;

            var itemDef = _itemCache.GetItem(item.baseName);

            if (itemDef == null || string.IsNullOrEmpty(itemDef.Name))
                return null;

            var indexItem = new IndexItem();
            indexItem.Owner = character.Header.name;
            indexItem.LevelRequirement = itemDef.LevelRequirement;
            indexItem.Searchable = itemDef.Name.ToLower();
            indexItem.Source = itemDef;
            indexItem.ItemName = itemDef.Name;
            indexItem.SourceInstance = item;
            return indexItem;
        }

        private bool FilterMatch(IndexItem item, IndexFilter filter)
        {
            if (filter.MaxLevel != null && item.LevelRequirement > filter.MaxLevel)
                return false;

            if (filter.MinLevel != null && item.LevelRequirement < filter.MinLevel)
                return false;

            if (filter.IsEquipped != null && item.IsEquipped != filter.IsEquipped)
                return false;

            return true;
        }
    }
}
