using GrimSearch.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Tests.Common
{
    [TestClass]
    public class VdfFileReaderTests
    {
        [TestMethod]
        public void TestConvertVdfToJson()
        {
            var fileContent = File.ReadAllText("Resources\\config.vdf");
            var jsonContent = VdfFileReader.ToJson(fileContent);

            var jsonObject = JsonConvert.DeserializeObject(jsonContent);

            Assert.IsNotNull(jsonObject);
        }
    }
}
