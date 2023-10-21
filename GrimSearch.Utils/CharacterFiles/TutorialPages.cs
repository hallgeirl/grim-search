using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class TutorialPages
    {
        public List<UInt32> pages = new List<UInt32>();
        public void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 15)
                throw new Exception();

            if (file.ReadInt() != 1) // version
                throw new Exception();


            UInt32 n = file.ReadInt();

            for (var i = 0; i<n; i++)
	        {
                pages.Add(file.ReadInt());
	        }

            file.ReadBlockEnd(b);
        }
    }
}
