using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class Inventory
    {
        public List<InventorySack> Sacks = new List<InventorySack>();
        public InventoryEquipment[] Equipment = new InventoryEquipment[12];
        public InventoryEquipment[] Weapon1 = new InventoryEquipment[2];
        public InventoryEquipment[] Weapon2 = new InventoryEquipment[2];
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
                    Sacks.Add(invSack);
                }

                useAlternate = file.ReadByte();

		        for (var i = 0; i < 12; i++)
		        {
                    Equipment[i] = new InventoryEquipment();
                    Equipment[i].Read(file);
                }

                alternate1 = file.ReadByte();

		        for (var i = 0; i< 2; i++)
		        {
                    Weapon1[i] = new InventoryEquipment();
                    Weapon1[i].Read(file);
		        }

		        alternate2 = file.ReadByte();

		        for (var i = 0; i< 2; i++)
		        {
                    Weapon2[i] = new InventoryEquipment();
                    Weapon2[i].Read(file);
		        }
	        }

	        file.ReadBlockEnd(b);
        }
    }
}
