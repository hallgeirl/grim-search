using System;
using System.Collections.Generic;
using System.IO;

namespace GrimSearch.Utils.CharacterFiles
{
    public class ReagentStashFile
    {
        public List<StashItem> items = new List<StashItem>();

        public void Read(Stream stream)
        {
            items.Clear();

            var file = new GDFileReader(stream);
            file.BeginRead();

            if (file.ReadInt() != 1)
                throw new InvalidDataException("Invalid reagent stash file header.");

            var outerBlock = new Block();
            var blockType = file.ReadBlockStart(outerBlock);
            if (blockType != 20)
                throw new InvalidDataException("Invalid reagent stash block type. Expected: 20, was " + blockType);

            var version = file.ReadInt();
            if (version != 1)
                throw new InvalidDataException("Unsupported reagent stash version: " + version);

            var unknown = file.ReadInt(false);
            if (unknown != 0)
                throw new InvalidDataException("Invalid reagent stash header value: " + unknown);

            GDString.Read(file); // Mod label.

            var numberOfItems = file.ReadInt();
            if (numberOfItems > 10000)
                throw new InvalidDataException("Invalid number of reagent stash items: " + numberOfItems);

            for (var i = 0; i < numberOfItems; i++)
            {
                var itemBlock = new Block();
                var itemBlockType = file.ReadBlockStart(itemBlock);
                if (itemBlockType != 0)
                    throw new InvalidDataException("Invalid reagent stash item block type: " + itemBlockType);

                var item = new StashItem
                {
                    baseName = GDString.Read(file),
                    stackCount = file.ReadInt()
                };
                items.Add(item);

                file.ReadBlockEnd(itemBlock);
            }

            file.ReadBlockEnd(outerBlock);
        }

        public StashPage ToStashPage()
        {
            return new StashPage
            {
                items = items,
                width = 0,
                height = 0,
                name = "Components and crafting materials"
            };
        }
    }
}
