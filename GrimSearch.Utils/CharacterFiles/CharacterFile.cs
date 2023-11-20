using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class CharacterFile : ICharacterFile
    {
        public Header Header { get; set; }
        public Uid Id = new Uid();
        public CharacterInfo Info = new CharacterInfo();
        public CharacterBio Bio = new CharacterBio();
        public Inventory Inventory { get; set; }
        public CharacterStash Stash = new CharacterStash();

        public CharacterFile()
        {
            Header = new Header();
            Inventory = new Inventory();
        }

        public void Read(Stream f)
        {
            var file = new GDFileReader(f);

            file.BeginRead();

            uint temp = file.ReadInt();
            if (temp != 0x58434447)
                throw new Exception();

            Header.Read(file);

            if (file.NextInt() != 0) //Checksum(?)
                throw new Exception();

            var fileVersion = file.ReadInt();
            if (fileVersion < 6 || fileVersion > 8) // version (6, 7 and 8 - only 8 supported here)
                throw new Exception("Invalid file version: " + fileVersion);

            Id.Read(file);

            Info.Read(file);
            Bio.Read(file);
            Inventory.Read(file);
            Stash.Read(file);

            // There's more in the character file, but we don't really care about it.
        }
    }
}
