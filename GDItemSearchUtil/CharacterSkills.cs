using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class CharacterSkills
    {
        public List<Skill> skills;
        public List<ItemSkill> itemSkills;
        public UInt32 masteriesAllowed;
        public UInt32 skillReclamationPointsUsed;
        public UInt32 devotionReclamationPointsUsed;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 8)
                throw new Exception();

            if (file.ReadInt() != 5) // version
                throw new Exception();

            skills = GDArray<Skill>.Read(file);
            masteriesAllowed = file.ReadInt();
            skillReclamationPointsUsed = file.ReadInt();
            devotionReclamationPointsUsed = file.ReadInt();
            itemSkills = GDArray<ItemSkill>.Read(file);

            file.ReadBlockEnd(b);
        }
    }
}
