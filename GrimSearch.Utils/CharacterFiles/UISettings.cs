using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class UISettings
    {
        public HotSlot[] slots;
        public string[] unknown4 = new string[5];
	    public string[] unknown5 = new string[5];
	    public UInt32 unknown2;
        public float cameraDistance;
        public byte[] unknown6 = new byte[5];
        public byte unknown1;
        public byte unknown3;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 14)
                throw new Exception();

            var version = file.ReadInt();
            if (version != 5 && version != 4) // version
                throw new Exception("Invalid save file version.");

            unknown1 = file.ReadByte();
            unknown2 = file.ReadInt();
            unknown3 = file.ReadByte();

            for (var i = 0; i < 5; i++)
            {
                unknown4[i] = GDString.Read(file);
                unknown5[i] = GDString.Read(file);
                unknown6[i] = file.ReadByte();
            }

            int numberOfSlots = 46;
            if (version == 4)
                numberOfSlots = 36;
            slots = new HotSlot[numberOfSlots];

            for (var i = 0; i < numberOfSlots; i++)
            {
                slots[i] = new HotSlot();
                slots[i].Read(file);
            }

            cameraDistance = file.ReadFloat();

            file.ReadBlockEnd(b);
        }
    }
}
