using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class InventoryEquipment : Item
    {
        public byte attached;

        public InventoryEquipment()
        {
        }

        public InventoryEquipment(UInt32 formatVersion) : base(formatVersion)
        {
        }

        public override void Read(GDFileReader file)
        {
            base.Read(file);

            attached = file.ReadByte();
        }
    }
}
