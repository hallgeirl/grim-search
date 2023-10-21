using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public static class GDWString
    {
        public static string Read(GDFileReader file)
        {
            UInt32 len = file.ReadInt();

            string ret = "";
            for (var i = 0; i < len; i++)
            {
                var b1 = file.ReadByte();
                var b2 = file.ReadByte();//Todo: use the second byte

                ret += (char)b1;
            }

            return ret;
        }

    }
}
