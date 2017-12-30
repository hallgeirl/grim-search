using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public class HotSlot : Readable
    {
        public string skill;
	    public string item;
	    public string bitmapUp;
	    public string bitmapDown;
	    public string label; //wstring
        public UInt32 type;
        public UInt32 equipLocation;
        public byte isItemSkill;

        public override void Read(GDFileReader file)
        {
            type = file.ReadInt();

            if (type == 0)
            {
                skill = GDString.Read(file);
                isItemSkill = file.ReadByte();
                item = GDString.Read(file);
                equipLocation = file.ReadInt();
            }
            else if (type == 4)
            {
                item = GDString.Read(file);
                bitmapUp = GDString.Read(file);
                bitmapDown = GDString.Read(file);
                label = GDWString.Read(file);
            }
        }
    }
}
