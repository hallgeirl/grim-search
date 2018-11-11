using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class StashItem : Item
    {
        public float x;
        public float y;

        public override void Read(GDFileReader file)
        {
            base.Read(file);
            x = file.ReadFloat();
            y = file.ReadFloat();
        }
    }
}
