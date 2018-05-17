using GDItemSearch.Utils.CharacterFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Tests.FileUtils
{
    [TestClass]
    public class TransferStashFileTests
    {
        [TestMethod]
        public void TestReadTransferStash()
        {
            TransferStashFile stash = new TransferStashFile();

            using (var s = File.OpenRead("Resources\\Saves\\transfer.gst"))
            {
                stash.Read(s);
            }
        }
    }
}
