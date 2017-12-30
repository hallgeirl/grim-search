using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class CharacterStash
    {
        public List<StashPage> stashPages;
        public UInt32 numStashPages;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 4)
                throw new Exception();

            if (file.ReadInt() != 6) // version
                throw new Exception();

            stashPages = GDArray<StashPage>.Read(file);

            file.ReadBlockEnd(b);
        }
    }
}
