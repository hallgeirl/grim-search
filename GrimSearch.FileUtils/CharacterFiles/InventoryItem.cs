using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class InventoryItem : Item
    {
        public UInt32 x;
        public UInt32 y;

        public override void Read(GDFileReader file)
        {
            base.Read(file);
            x = file.ReadInt();
            y = file.ReadInt();
        }
    }
}
