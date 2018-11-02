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
    public class BlueprintFileTests
    {
        [TestMethod]
        public void TestReadBlueprintFile()
        {
            BlueprintFile blueprints = new BlueprintFile();

            using (var s = File.OpenRead("Resources\\Saves\\formulas.gst"))
            {
                blueprints.Read(s);
            }
        }
    }
}
