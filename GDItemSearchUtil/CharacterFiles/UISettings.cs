using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class UISettings
    {
        public HotSlot[] slots = new HotSlot[46];
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

            if (file.ReadInt() != 5) // version
                throw new Exception();

            unknown1 = file.ReadByte();
            unknown2 = file.ReadInt();
            unknown3 = file.ReadByte();

            for (var i = 0; i < 5; i++)
            {
                unknown4[i] = GDString.Read(file);
                unknown5[i] = GDString.Read(file);
                unknown6[i] = file.ReadByte();
            }

            for (var i = 0; i < 46; i++)
            {
                slots[i] = new HotSlot();
                slots[i].Read(file);
            }

            cameraDistance = file.ReadFloat();

            file.ReadBlockEnd(b);
        }
    }
}
