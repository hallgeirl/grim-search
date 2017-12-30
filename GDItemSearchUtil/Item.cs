using System;
using System.IO;

namespace GDItemSearchUtil
{
    public class Item : Readable
    {
	    public string baseName;
        public string prefixName;
        public string suffixName;
        public string modifierName;
        public string transmuteName;
        public string relicName;
        public string relicBonus;
        public string augmentName;
        public UInt32 stackCount;
        public UInt32 seed;
        public UInt32 relicSeed;
        public UInt32 unknown;
        public UInt32 augmentSeed;
        public UInt32 var1;

        public override void Read(GDFileReader file)
        {
            baseName = GDString.Read(file);
            prefixName = GDString.Read(file);
            suffixName = GDString.Read(file);
            modifierName = GDString.Read(file);
            transmuteName = GDString.Read(file);
            seed = file.ReadInt();
            relicName = GDString.Read(file);
            relicBonus = GDString.Read(file);
            relicSeed = file.ReadInt();
            augmentName = GDString.Read(file);
            unknown = file.ReadInt();
            augmentSeed = file.ReadInt();
            var1 = file.ReadInt();
            stackCount = file.ReadInt();
        }
    }
}
