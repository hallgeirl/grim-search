using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class LoreNotes
    {
        public List<string> names;

        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 12)
                throw new Exception();

            if (file.ReadInt() != 1) // version
                throw new Exception();

            names = new List<string>();
            UInt32 n = file.ReadInt();

            for (var i = 0; i < n; i++)
            {
                var name = GDString.Read(file);
                names.Add(name);
            }

            file.ReadBlockEnd(b);
        }
    }
}
