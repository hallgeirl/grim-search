using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class CharacterFile
    {
        public Header Header = new Header();
        public Uid Id = new Uid();
        public CharacterInfo Info = new CharacterInfo();
        public CharacterBio Bio = new CharacterBio();
        public Inventory Inventory = new Inventory();
        public CharacterStash Stash = new CharacterStash();
        private RespawnList respawns = new RespawnList();
        private TeleportList teleports = new TeleportList();
        private MarkerList markers = new MarkerList();
        private ShrineList shrines = new ShrineList();
        private CharacterSkills skills = new CharacterSkills();
        private LoreNotes notes = new LoreNotes();
        private FactionPack factions = new FactionPack();
        private UISettings ui = new UISettings();
        private TutorialPages tutorials = new TutorialPages();
        private PlayStats stats = new PlayStats();
        private TriggerTokens tokens = new TriggerTokens();

        public void Read(Stream f)
        {
            var file = new GDFileReader(f);

            file.BeginRead();

            if (file.ReadInt() != 0x58434447)
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
            respawns.Read(file);
            teleports.Read(file);
            markers.Read(file);
            shrines.Read(file);
            skills.Read(file);
            notes.Read(file);
            factions.Read(file);
            ui.Read(file);
            tutorials.Read(file);
            stats.Read(file);

            if (fileVersion >= 7)
                tokens.Read(file);


            file.EndRead();
        }

        
    }
}
