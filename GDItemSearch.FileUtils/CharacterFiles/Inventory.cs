using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class Inventory
    {
        public List<InventorySack> sacks = new List<InventorySack>();
        public InventoryEquipment[] equipment = new InventoryEquipment[12];
        public InventoryEquipment[] weapon1 = new InventoryEquipment[2];
        public InventoryEquipment[] weapon2 = new InventoryEquipment[2];
        public UInt32 focused;
        public UInt32 selected;
        public byte flag;
        public byte useAlternate;
        public byte alternate1;
        public byte alternate2;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 3)
                throw new Exception();

            if (file.ReadInt() != 4) // version
                throw new Exception();

            if ((flag = file.ReadByte()) != 0)
            {
                UInt32 n = file.ReadInt();
                focused = file.ReadInt();
		        selected = file.ReadInt();

		        for (UInt32 i = 0; i<n; i++)
		        {
                    var invSack = new InventorySack();
                    invSack.Read(file);
                    sacks.Add(invSack);
                }

                useAlternate = file.ReadByte();

		        for (var i = 0; i < 12; i++)
		        {
                    equipment[i] = new InventoryEquipment();
                    equipment[i].Read(file);
                }

                alternate1 = file.ReadByte();

		        for (var i = 0; i< 2; i++)
		        {
                    weapon1[i] = new InventoryEquipment();
                    weapon1[i].Read(file);
		        }

		        alternate2 = file.ReadByte();

		        for (var i = 0; i< 2; i++)
		        {
                    weapon2[i] = new InventoryEquipment();
                    weapon2[i].Read(file);
		        }
	        }

	        file.ReadBlockEnd(b);
        }
    }
}
