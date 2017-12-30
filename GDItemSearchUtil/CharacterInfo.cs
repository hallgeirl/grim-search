using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class CharacterInfo
    {
        public string texture;
        public UInt32 money;
        public UInt32 lootMode;
        public UInt32 currentTribute;
        public byte isInMainQuest;
        public byte hasBeenInGame;
        public byte difficulty;
        public byte greatestDifficulty;
        public byte greatestSurvivalDifficulty;
        public byte compassState;
        public byte skillWindowShowHelp;
        public byte alternateConfig;
        public byte alternateConfigEnabled;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 1)
                throw new Exception();

            if (file.ReadInt() != 4) // version
                throw new Exception();

            isInMainQuest = file.ReadByte();
            hasBeenInGame = file.ReadByte();
            difficulty = file.ReadByte();
            greatestDifficulty = file.ReadByte();
            money = file.ReadInt();
            greatestSurvivalDifficulty = file.ReadByte();
            currentTribute = file.ReadInt();
            compassState = file.ReadByte();
            lootMode = file.ReadInt();
            skillWindowShowHelp = file.ReadByte();
            alternateConfig = file.ReadByte();
            alternateConfigEnabled = file.ReadByte();
            texture = GDString.Read(file);

            file.ReadBlockEnd(b);
        }
    }
}
