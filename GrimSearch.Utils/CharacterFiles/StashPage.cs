using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.CharacterFiles
{
    public class StashPage : Readable
    {
        private readonly UInt32 formatVersion;

        public List<StashItem> items;
        public UInt32 width;
        public UInt32 height;
        public UInt32 borderIndex;
        public UInt32 borderColorIndex;
        public UInt32 symbolIndex;
        public UInt32 symbolColorIndex;
        public string name;

        public StashPage() : this(5)
        {
        }

        public StashPage(UInt32 formatVersion)
        {
            this.formatVersion = formatVersion;
        }

        public override void Read(GDFileReader file)
        {
            Block b = new Block();

            var blockType = file.ReadBlockStart(b);
            if (blockType != 0)
                throw new InvalidDataException("Invalid stash page block type: " + blockType);

            width = file.ReadInt();
            height = file.ReadInt();
            var numberOfItems = file.ReadInt();
            items = new List<StashItem>();
            for (var i = 0; i < numberOfItems; i++)
            {
                var item = new StashItem(formatVersion);
                item.Read(file);
                items.Add(item);
            }

            if (formatVersion >= 9)
            {
                borderIndex = file.ReadInt();
                borderColorIndex = file.ReadInt();
                symbolIndex = file.ReadInt();
                symbolColorIndex = file.ReadInt();
                name = GDWString.Read(file);
            }

            file.ReadBlockEnd(b);
        }
    }
}
