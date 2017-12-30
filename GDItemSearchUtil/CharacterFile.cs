using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class CharacterFile
    {
        public Header hdr = new Header();
        public Uid id = new Uid();
        public CharacterInfo info = new CharacterInfo();
        public CharacterBio bio = new CharacterBio();
        public Inventory inv = new Inventory();
        public CharacterStash stash = new CharacterStash();
        public RespawnList respawns = new RespawnList();
        public TeleportList teleports = new TeleportList();
        public MarkerList markers = new MarkerList();
        public ShrineList shrines = new ShrineList();
        public CharacterSkills skills = new CharacterSkills();
        public LoreNotes notes = new LoreNotes();
        public FactionPack factions = new FactionPack();
        public UISettings ui = new UISettings();
        public TutorialPages tutorials = new TutorialPages();
        public PlayStats stats = new PlayStats();
        public TriggerTokens tokens = new TriggerTokens();

        public void Read(Stream f)
        {
            var file = new GDFileReader(f);

            file.BeginRead();

            if (file.ReadInt() != 0x58434447)
                throw new Exception();

            if (file.ReadInt() != 2) // Header version. Must be 2.
                throw new Exception();

            hdr.Read(file);

            if (file.NextInt() != 0) //Checksum(?)
                throw new Exception();

            if (file.ReadInt() != 8) // version (6, 7 and 8 - only 8 supported here)
                throw new Exception();

            id.Read(file);

            info.Read(file);
            bio.Read(file);
            inv.Read(file);
            stash.Read(file);
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
            tokens.Read(file);

            file.EndRead();
        }

        
    }
}
