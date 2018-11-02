using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils.CharacterFiles
{
    public class BlueprintFile : ICharacterFile
    {
        private SortedSet<string> blueprints = new SortedSet<string>();
        // May not have been included in the recipe file, always craftable
        const string relicCalamity = "records/items/crafting/blueprints/relic/craft_relic_b001.dbr";
        const string relicRuination = "records/items/crafting/blueprints/relic/craft_relic_b002.dbr";
        const string relicEquilibrium = "records/items/crafting/blueprints/relic/craft_relic_b003.dbr";

        private static string ReadString(Stream f)
        {
            uint length = ReadUInteger(f);
            long remaining = f.Length - f.Position;
            if (length > remaining || length > 1024 * 10 || length <= 0)
            {
                //throw new ArgumentOutOfRangeException(String.Format("Could not parse string of length {0}", length));
                return string.Empty;
            }

            char[] sub = new char[length];
            byte[] buf = new byte[length];
            if (f.Read(buf, 0, (int)length) != length)
            {
                throw new Exception("ReadString requested {0} bytes but only got " + length + " bytes");
            }
            for (int i = 0; i < length; i++)
            {
                sub[i] = Convert.ToChar(buf[i]);
            }

            return new String(sub);
        }

        private static uint ReadUInteger(Stream f, bool endian = false)
        {
            byte[] array = new byte[4];
            if (f.Read(array, 0, 4) != 4)
            {
                throw new Exception(string.Format("ReadUInteger called with only {0} bytes remaining!", f.Length - f.Position));
            }
            else
            {
                if (endian && BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array);
                }
                return BitConverter.ToUInt32(array, 0);
            }
        }

        public void Read(Stream f)
        {
            bool isExpansion = false;

            if (!"begin_block".Equals(ReadString(f)))
                throw new Exception("begin_block not found in beginning of recipe file.");

            ReadUInteger(f);

            if (!"formulasVersion".Equals(ReadString(f)))
                throw new Exception("formulasVersion not found in recipe file.");
            uint version = ReadUInteger(f);

            if (!"numEntries".Equals(ReadString(f)))
                throw new Exception("numEntries not found in recipe file.");
            uint numItems = ReadUInteger(f);

            //uint unknown2 = IOHelper.ReadUInteger(fs);

            if (version >= 3)
            {
                if (!"expansionStatus".Equals(ReadString(f)))
                    throw new Exception("expansionStatus not found in recipe file.");

                isExpansion = f.ReadByte() != 0;
            }


            for (uint i = 0; i < numItems; i++)
            {
                if (!"itemName".Equals(ReadString(f)))
                    throw new Exception("itemName not found in recipe file for item " + i);

                blueprints.Add(ReadString(f));

                if (!"formulaRead".Equals(ReadString(f)))
                    throw new Exception("formulaRead not found in recipe file for item " + i);

                ReadUInteger(f);
            }

            if (!"end_block".Equals(ReadString(f)))
                throw new Exception("end_block not found in end of recipe file.");

            blueprints.Add(relicCalamity);
            blueprints.Add(relicRuination);
            blueprints.Add(relicEquilibrium);
        }

        public CharacterFile ToCharacterFile()
        {
            return new CharacterFile()
            {
                Header = new Header() { Name = "Blueprints" },
                Inventory = new Inventory()
                {
                    Sacks = new List<InventorySack>()
                    {
                        new InventorySack() { Items = blueprints.Select(x=>BlueprintToInventoryItem(x)).ToList()  }
                    }
                }
            };
        }

        private InventoryItem BlueprintToInventoryItem(string blueprintString)
        {
            return new InventoryItem()
            {
                baseName = blueprintString
            };
        }
    }
}
