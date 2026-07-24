using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class InventorySack
    {
        private readonly UInt32 formatVersion;

        public List<InventoryItem> Items = new List<InventoryItem>();
        byte tempBool;

        public InventorySack() : this(5)
        {
        }

        public InventorySack(UInt32 formatVersion)
        {
            this.formatVersion = formatVersion;
        }

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 0)
                throw new Exception();

            tempBool = file.ReadByte();
            var numberOfItems = file.ReadInt();
            Items = new List<InventoryItem>();
            for (var i = 0; i < numberOfItems; i++)
            {
                var item = new InventoryItem(formatVersion);
                item.Read(file);
                Items.Add(item);
            }

            file.ReadBlockEnd(b);
        }
    }
}
