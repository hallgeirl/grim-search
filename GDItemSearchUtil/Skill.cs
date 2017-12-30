using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class Skill : Readable
    {

        public string name;
        public string autoCastSkill;
        public string autoCastController;
        public UInt32 level;
        public UInt32 devotionLevel;
        public UInt32 experience;
        public UInt32 active;
        public byte enabled;
        public byte unknown1;
        public byte unknown2;

        public override void Read(GDFileReader file)
        {
            name = GDString.Read(file);
            level = file.ReadInt();
            enabled = file.ReadByte();
            devotionLevel = file.ReadInt();
            experience = file.ReadInt();
            active = file.ReadInt();
            unknown1 = file.ReadByte();
            unknown2 = file.ReadByte();
            autoCastSkill = GDString.Read(file);
            autoCastController = GDString.Read(file);
        }
    };
}
