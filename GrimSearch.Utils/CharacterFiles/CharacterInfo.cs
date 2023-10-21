﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
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

            var version = file.ReadInt();
            if (version < 3 || version > 5) // version
                throw new Exception("Invalid character info version");

            isInMainQuest = file.ReadByte();
            hasBeenInGame = file.ReadByte();
            difficulty = file.ReadByte();
            greatestDifficulty = file.ReadByte();
            money = file.ReadInt();
            if (version >= 4)
            {
                greatestSurvivalDifficulty = file.ReadByte();
                currentTribute = file.ReadInt();    
            }
            
            compassState = file.ReadByte();

            if (version >= 2 && version <= 4)
                lootMode = file.ReadInt();

            skillWindowShowHelp = file.ReadByte();
            alternateConfig = file.ReadByte();
            alternateConfigEnabled = file.ReadByte();
            texture = GDString.Read(file);
            if (version >= 5)
            {
                uint size = file.ReadInt(true);

                var lootFilters = new byte[size];
                for (int i = 0; i < lootFilters.Length; i ++)
                {
                    lootFilters[i] = file.ReadByte();
                }
            }

            file.ReadBlockEnd(b);
        }
    }
}
