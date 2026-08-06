using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class CharacterFile : ICharacterFile
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public UInt32 FormatVersion { get; private set; }
        public Header Header { get; set; }
        public Uid Id = new Uid();
        public CharacterInfo Info = new CharacterInfo();
        public CharacterBio Bio = new CharacterBio();
        public PlayStats Stats = new PlayStats();
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

            FormatVersion = file.ReadInt();
            Logger.Info(
                "Character file {Path}: format version {FormatVersion}, header version {HeaderVersion}",
                f is FileStream fileStream ? fileStream.Name : "<stream>",
                FormatVersion,
                Header.Version);
            if (FormatVersion < 6 || FormatVersion > 8) // version (6, 7 and 8 - only 8 supported here)
                throw new Exception("Invalid file version: " + FormatVersion);

            Id.Read(file);

            Info.Read(file);
            Bio.Read(file);
            Inventory.Read(file);
            Stash.Read(file);

            // The intervening blocks are not needed, but play stats contain the
            // death count used to identify dead hardcore characters.
            if (file.TryAdvanceToBlock(16))
                Stats.ReadDeathCount(file);
        }

        public bool IsHardcore => Header.Hardcore != 0;
        public bool IsDeadHardcore => IsHardcore && Stats.deaths > 0;
    }
}
