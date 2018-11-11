using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class ItemSkill : Readable
    {

        public string name;
        public string autoCastSkill;
        public string autoCastController;
        public string itemName;
        public UInt32 itemSlot;

        public override void Read(GDFileReader file)
        {
            name = GDString.Read(file);
            autoCastSkill = GDString.Read(file);
            autoCastController = GDString.Read(file);
            itemSlot = file.ReadInt();
            itemName = GDString.Read(file);
        }
    };
}
