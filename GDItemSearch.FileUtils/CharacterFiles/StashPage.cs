using System;
using System.Collections.Generic;
using System.Text;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class StashPage : Readable
    {
        public List<StashItem> items;
        public UInt32 width;
        public UInt32 height;

        public override void Read(GDFileReader file)
        {
            Block b = new Block();

            if (file.ReadBlockStart(b) != 0)
                throw new Exception();

            width = file.ReadInt();
            height = file.ReadInt();
            items = GDArray<StashItem>.Read(file);

            file.ReadBlockEnd(b);
        }
    }
}
