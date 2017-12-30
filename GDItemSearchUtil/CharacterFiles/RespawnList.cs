using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class RespawnList
    {
        public List<Uid>[] uids = new List<Uid>[3];
        public Uid[] spawn = new Uid[3];

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 5)
                throw new Exception();

            if (file.ReadInt() != 1) // version
                throw new Exception();

            for (var i = 0; i < 3; i++)
            {
                uids[i] = GDArray<Uid>.Read(file);
            }

            for (var i = 0; i < 3; i++)
            {
                spawn[i] = new Uid();
                spawn[i].Read(file);
            }

            file.ReadBlockEnd(b);
        }
    }
}
