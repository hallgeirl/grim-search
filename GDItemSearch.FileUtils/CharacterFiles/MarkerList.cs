using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class MarkerList
    {
        public List<Uid>[] uids = new List<Uid>[3];
        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 7)
                throw new Exception();

            if (file.ReadInt() != 1) // version
                throw new Exception();

            for (var i = 0; i < 3; i++)
            {
                uids[i] = GDArray<Uid>.Read(file);
            }

            file.ReadBlockEnd(b);
        }
    }
}
