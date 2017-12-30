using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
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
            var headerVersion = file.ReadInt();
            if (headerVersion != 1 && headerVersion != 2) // Header version. Must be 2.
                throw new Exception("Invalid header version.");

            name = GDWString.Read(file);
            sex = file.ReadByte();
            tag = GDString.Read(file);
            level = file.ReadInt();
            hardcore = file.ReadByte();
            if (headerVersion == 2)
                file.ReadByte(); //expansion status
        }
    }
}
