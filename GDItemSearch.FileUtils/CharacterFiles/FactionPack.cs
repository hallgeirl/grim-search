using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class FactionPack
    {
        public List<FactionData> factions;
        public UInt32 faction;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 13)
                throw new Exception();

            if (file.ReadInt() != 5) // version
                throw new Exception();

            faction = file.ReadInt();
            factions = GDArray<FactionData>.Read(file);

            file.ReadBlockEnd(b);
        }
    }
}
