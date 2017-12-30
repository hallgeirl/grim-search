using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class TriggerTokens
    {
        public List<string>[] tokens = new List<string>[3];

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 10)
                throw new Exception();

            if (file.ReadInt() != 2) // version
                throw new Exception();

            for (var i = 0; i < 3; i++)
            {
                tokens[i] = new List<string>();
                UInt32 n = file.ReadInt();

                for (var j = 0; j < n; j++)
                {
                    var token = GDString.Read(file);
                    tokens[i].Add(token);
                }
            }

            file.ReadBlockEnd(b);
        }
    }
}
