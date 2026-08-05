using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.CharacterFiles
{
    public class TransferStashFile : ICharacterFile
    {
        private static readonly UInt32[] SupportedVersions = { 4, 5, 8, 9, 11 };

        public List<StashPage> sacks = new List<StashPage>();

        public void Read(Stream s)
        {
            string mod;
            sacks.Clear();

            var file = new GDFileReader(s);
            file.BeginRead();

            if (file.ReadInt() != 2)
                throw new InvalidDataException("Invalid transfer stash file header.");

            Block b = new Block();
            var bstart = file.ReadBlockStart(b);
            if (bstart != 18)
            {
                throw new InvalidDataException("Invalid transfer stash block type. Expected: 18, was " + bstart);
            }

            var version = file.ReadInt();
            if (!SupportedVersions.Contains(version))
                throw new InvalidDataException("Unsupported transfer stash version: " + version);

            var unknown = file.ReadInt(false);
            if (unknown != 0)
            {
                throw new InvalidDataException("Invalid transfer stash header value: " + unknown);
            }
            
            mod = GDString.Read(file);
            if (version >= 5)
                file.ReadByte(); // Expansion bitmask.

            uint numberOfSacks = file.ReadInt();
            if (numberOfSacks > 100)
                throw new InvalidDataException("Invalid number of transfer stash pages: " + numberOfSacks);

            for (int i = 0; i < numberOfSacks; i++)
            {
                var stashPage = new StashPage(version);
                stashPage.Read(file);

                sacks.Add(stashPage);
            }

            file.ReadBlockEnd(b);
        }

        public CharacterFile ToCharacterFile(string name = "Transfer stash", bool isHardcore = false)
        {
            return new CharacterFile()
            {
                Header = new Header()
                {
                    Name = name,
                    Hardcore = isHardcore ? (byte)1 : (byte)0
                },
                Stash = new CharacterStash() { stashPages = sacks, numStashPages = (uint)sacks.Count }
            };
        }
    }
}
