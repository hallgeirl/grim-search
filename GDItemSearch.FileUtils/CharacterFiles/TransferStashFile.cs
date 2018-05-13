using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class TransferStashFile : ICharacterFile
    {
        public List<StashPage> sacks = new List<StashPage>();

        public void Read(Stream s)
        {
            string mod;

            var file = new GDFileReader(s);
            file.BeginRead();

            if (file.ReadInt() != 2)
                throw new IOException();

            Block b = new Block();
            var bstart = file.ReadBlockStart(b);
            if (bstart != 18)
            {
                throw new Exception("An error occured while reading transfer stash. Expected: 18, was " + bstart);
            }

            var version = file.ReadInt();
            if (version < 4)
                throw new Exception("Transfer stash error: Invalid version: " + version);

            var unknown = file.ReadInt(false);
            if (unknown != 0)
            {
                throw new Exception("An error occured while reading transfer stash. Expected: 0, was something else.");
            }
            
            mod = GDString.Read(file);
            if (version == 5)
            {
                var isExp = file.ReadByte();
            }

            uint numberOfSacks = file.ReadInt();

            for (int i = 0; i < numberOfSacks; i++)
            {
                var stashPage = new StashPage();
                stashPage.Read(file);

                sacks.Add(stashPage);
            }

            file.ReadBlockEnd(b);
        }

        public CharacterFile ToCharacterFile()
        {
            return new CharacterFile()
            {
                Header = new Header()
                {
                    Name = "Transfer stash"
                },
                Stash = new CharacterStash() { stashPages = sacks, numStashPages = (uint)sacks.Count }
            };
        }
    }
}
