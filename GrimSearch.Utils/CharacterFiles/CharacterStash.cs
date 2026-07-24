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
            if (version == 11)
            {
                stashPages = ReadStashPages(file, version);
            }
            else if (version == 6)
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
                throw new InvalidDataException("Invalid stash version: " + version);

            file.ReadBlockEnd(b);
        }

        private static List<StashPage> ReadStashPages(GDFileReader file, UInt32 version)
        {
            var numberOfPages = file.ReadInt();
            var pages = new List<StashPage>();
            for (var i = 0; i < numberOfPages; i++)
            {
                var page = new StashPage(version);
                page.Read(file);
                pages.Add(page);
            }
            return pages;
        }
    }
}
