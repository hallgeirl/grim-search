using GrimSearch.Utils.CharacterFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class TransferStashFileTests
    {
        [TestMethod]
        public void TestReadTransferStash_Pre_1_3_0()
        {
            TransferStashFile stash = new TransferStashFile();

            using (var s = File.OpenRead("Resources/Saves/transfer.gst"))
            {
                stash.Read(s);
            }

            Assert.AreEqual(5, stash.sacks.Count);
        }

        [TestMethod]
        public void TestReadTransferStash_1_3_0()
        {
            TransferStashFile stash = new TransferStashFile();

            using (var s = File.OpenRead("Resources/Saves/1.3.0/transfer.gst"))
            {
                stash.Read(s);
            }

            Assert.AreEqual(6, stash.sacks.Count);
            Assert.AreEqual(104, stash.sacks.Sum(x => x.items.Count));
            Assert.AreEqual((uint)19, stash.sacks[0].height);
            Assert.AreEqual("records/items/upgraded/gearweapons/guns1h/c033_gun1h.dbr",
                stash.sacks[0].items[0].baseName);
        }

        [TestMethod]
        public void TestReadReagentStash_1_3_0()
        {
            var stash = new ReagentStashFile();

            using (var s = File.OpenRead("Resources/Saves/1.3.0/reagents.gst"))
            {
                stash.Read(s);
            }

            Assert.AreEqual(86, stash.items.Count);
            Assert.AreEqual("records/items/crafting/materials/craft_aetherialmissive.dbr",
                stash.items[0].baseName);
            Assert.AreEqual((uint)92, stash.items[0].stackCount);
        }
    }
}
