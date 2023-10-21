using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class Header
    {
        public string Name;
        public string Tag;
        public UInt32 Level;
        public byte Sex;
        public byte Hardcore;

        public void Read(GDFileReader file)
        {
            var headerVersion = file.ReadInt();
            if (headerVersion != 1 && headerVersion != 2) // Header version. Must be 2.
                throw new Exception("Invalid header version.");

            Name = GDWString.Read(file);
            Sex = file.ReadByte();
            Tag = GDString.Read(file);
            Level = file.ReadInt();
            Hardcore = file.ReadByte();
            if (headerVersion == 2)
                file.ReadByte(); //expansion status
        }
    }
}
