using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class ShrineList
    {
        public List<Uid>[] uids = new List<Uid>[6];

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 17)
                throw new Exception();

            if (file.ReadInt() != 2) // version
                throw new Exception();

            for (var i = 0; i < 6; i++)
            {
                uids[i] = GDArray<Uid>.Read(file);
            }

            file.ReadBlockEnd(b);
        }
    }
}
