using System;
using System.IO;

namespace GrimSearch.Utils.CharacterFiles
{
    public class Item : Readable
    {
        private readonly UInt32 formatVersion;

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
        public string ascendantName;
        public string ascendantTwoHandName;
        public UInt32 seedRerolls;
        public UInt32 affixRerolls;

        public Item() : this(5)
        {
        }

        protected Item(UInt32 formatVersion)
        {
            this.formatVersion = formatVersion;
        }

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

            if (formatVersion >= 11)
            {
                unknown = file.ReadInt();
                augmentSeed = file.ReadInt();
                ascendantName = GDString.Read(file);
                ascendantTwoHandName = GDString.Read(file);
                var1 = file.ReadInt();
                stackCount = file.ReadInt();
                seedRerolls = file.ReadInt();
                affixRerolls = file.ReadInt();
                return;
            }

            if (formatVersion >= 8)
            {
                ascendantName = GDString.Read(file);
                ascendantTwoHandName = GDString.Read(file);
            }

            unknown = file.ReadInt();
            augmentSeed = file.ReadInt();
            var1 = file.ReadInt();
            stackCount = file.ReadInt();

            if (formatVersion >= 8)
                seedRerolls = file.ReadInt();
        }
    }
}
