using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class FactionData : Readable
    {
        public float value;
        public float positiveBoost;
        public float negativeBoost;
        public byte modified;
        public byte unlocked;

        public override void Read(GDFileReader file)
        {
            modified = file.ReadByte();
            unlocked = file.ReadByte();
            value = file.ReadFloat();
            positiveBoost = file.ReadFloat();
            negativeBoost = file.ReadFloat();
        }
    }
}
