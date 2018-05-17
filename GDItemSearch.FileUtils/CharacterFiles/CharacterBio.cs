using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class CharacterBio
    {
        public UInt32 level;
        public UInt32 experience;
        public UInt32 modifierPoints;
        public UInt32 skillPoints;
        public UInt32 devotionPoints;
        public UInt32 totalDevotion;
        public float physique;
        public float cunning;
        public float spirit;
        public float health;
        public float energy;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 2)
                throw new Exception();

            if (file.ReadInt() != 8) // version
                throw new Exception();

            level = file.ReadInt();
            experience = file.ReadInt();
            modifierPoints = file.ReadInt();
            skillPoints = file.ReadInt();
            devotionPoints = file.ReadInt();
            totalDevotion = file.ReadInt();
            physique = file.ReadFloat();
            cunning = file.ReadFloat();
            spirit = file.ReadFloat();
            health = file.ReadFloat();
            energy = file.ReadFloat();

            file.ReadBlockEnd(b);
        }
    }
}
