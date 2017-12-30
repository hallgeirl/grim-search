using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class PlayStats
    {
        public string[] greatestMonsterKilledName = new string[3];
	    public string[] lastMonsterHit = new string[3];
	    public string[] lastMonsterHitBy = new string[3];
	    public UInt32[] greatestMonsterKilledLevel = new UInt32[3];
        public UInt32[] greatestMonsterKilledLifeAndMana = new UInt32[3];
        public UInt32[] bossKills = new UInt32[3];
        public UInt32 playTime;
        public UInt32 deaths;
        public UInt32 kills;
        public UInt32 experienceFromKills;
        public UInt32 healthPotionsUsed;
        public UInt32 manaPotionsUsed;
        public UInt32 maxLevel;
        public UInt32 hitsReceived;
        public UInt32 hitsInflicted;
        public UInt32 criticalHitsInflicted;
        public UInt32 criticalHitsReceived;
        public UInt32 championKills;
        public UInt32 heroKills;
        public UInt32 itemsCrafted;
        public UInt32 relicsCrafted;
        public UInt32 transcendentRelicsCrafted;
        public UInt32 mythicalRelicsCrafted;
        public UInt32 shrinesRestored;
        public UInt32 oneShotChestsOpened;
        public UInt32 loreNotesCollected;
        public float greatestDamageInflicted;
        public float lastHit;
        public float lastHitBy;
        public float greatestDamageReceived;
        public UInt32 survivalWaveTier;
        public UInt32 greatestSurvivalScore;
        public UInt32 cooldownRemaining;
        public UInt32 cooldownTotal;
        public UInt32 unknown1;
        public UInt32 unknown2;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 16)
                throw new Exception();

            if (file.ReadInt() != 9) // version
                throw new Exception();

            playTime = file.ReadInt();
            deaths = file.ReadInt();
            kills = file.ReadInt();
            experienceFromKills = file.ReadInt();
            healthPotionsUsed = file.ReadInt();
            manaPotionsUsed = file.ReadInt();
            maxLevel = file.ReadInt();
            hitsReceived = file.ReadInt();
            hitsInflicted = file.ReadInt();
            criticalHitsInflicted = file.ReadInt();
            criticalHitsReceived = file.ReadInt();
            greatestDamageInflicted = file.ReadFloat();

            for (var i = 0; i < 3; i++)
            {
                greatestMonsterKilledName[i] = GDString.Read(file);
                greatestMonsterKilledLevel[i] = file.ReadInt();
                greatestMonsterKilledLifeAndMana[i] = file.ReadInt();
                lastMonsterHit[i] = GDString.Read(file);
                lastMonsterHitBy[i] = GDString.Read(file);
            }

            championKills = file.ReadInt();
            lastHit = file.ReadFloat();
            lastHitBy = file.ReadFloat();
            greatestDamageReceived = file.ReadFloat();
            heroKills = file.ReadInt();
            itemsCrafted = file.ReadInt();
            relicsCrafted = file.ReadInt();
            transcendentRelicsCrafted = file.ReadInt();
            mythicalRelicsCrafted = file.ReadInt();
            shrinesRestored = file.ReadInt();
            oneShotChestsOpened = file.ReadInt();
            loreNotesCollected = file.ReadInt();

            for (var i = 0; i < 3; i++)
            {
                bossKills[i] = file.ReadInt();
            }

            survivalWaveTier = file.ReadInt();
            greatestSurvivalScore = file.ReadInt();
            cooldownRemaining = file.ReadInt();
            cooldownTotal = file.ReadInt();

            unknown1 = file.ReadInt();
            unknown2 = file.ReadInt();

            file.ReadBlockEnd(b);
        }
    }
}
