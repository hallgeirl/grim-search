using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    static public class GDString
    {
        public static string Read(GDFileReader file)
        {
            UInt32 len = file.ReadInt();

            string ret = "";
            for (var i = 0; i < len; i++)
            {
                var b1 = file.ReadByte();

                ret += (char)b1;
            }

            return ret;
        }
    }
}
