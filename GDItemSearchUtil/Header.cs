using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class Header
    {
        public string name;
        public string tag;
        public UInt32 level;
        public byte sex;
        public byte hardcore;

        public void Read(GDFileReader file)
        {
            name = GDWString.Read(file);
            sex = file.ReadByte();
            tag = GDString.Read(file);
            level = file.ReadInt();
            hardcore = file.ReadByte();
            file.ReadByte(); //expansion status
        }
    }
}
