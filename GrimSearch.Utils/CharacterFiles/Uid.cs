using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class Uid : Readable
    {
        public byte[] id = new byte[16];

        public override void Read(GDFileReader file)
        {
            for (var i = 0; i < 16; i++)
            {
                id[i] = file.ReadByte();
            }
        }
    }
}
