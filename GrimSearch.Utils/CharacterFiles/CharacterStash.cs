using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class CharacterStash
    {
        // new version
        public List<StashPage> stashPages = new List<StashPage>();
        public UInt32 numStashPages;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 4)
                throw new Exception();

            var version = file.ReadInt();
            if (version == 6) // version
                stashPages = GDArray<StashPage>.Read(file);
            else if (version == 5)
            {
                stashPages = new List<StashPage>();
                var width = file.ReadInt();
                var height = file.ReadInt();
                var items = GDArray<StashItem>.Read(file);
                stashPages.Add(new StashPage() { width = width, height = height, items = items });
            }
            else
                throw new Exception("Invalid stash version.");

            file.ReadBlockEnd(b);
        }
    }
}
