﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class InventoryEquipment : Item
    {
        public byte attached;

        public override void Read(GDFileReader file)
        {
            base.Read(file);

            attached = file.ReadByte();
        }
    }
}
