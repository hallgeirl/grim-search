using GrimSearch.Utils.Steam;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;

namespace GrimSearch.Tests.Common
{
    [TestClass]
    public class VdfFileReaderTests
    {
        [TestMethod]
        public void TestConvertVdfToJson()
        {
            var fileContent = File.ReadAllText("Resources/config.vdf");
            var jsonContent = VdfFileReader.ToJson(fileContent);

            var jsonObject = JsonConvert.DeserializeObject(jsonContent);

            Assert.IsNotNull(jsonObject);
        }
    }
}
