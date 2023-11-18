using GrimSearch.Utils.CharacterFiles;
using GrimSearch.Utils.Steam;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Tests.Utils.Steam
{
    [TestClass]
    public class SteamConfigTests
    {
        [TestMethod]
        public void TestReadSteamConfigVdf()
        {
            var vdfToJson = VdfFileReader.ToJson(File.ReadAllText("Resources/config.vdf"));
            var steamConfig = JsonConvert.DeserializeObject<SteamConfig>(vdfToJson);

            Assert.AreEqual("C:\\Steamapps", steamConfig.Software.Valve.Steam["BaseInstallFolder_1"]);
        }

        [TestMethod]
        public void TestReadSteamRegistryVdf()
        {
            var steamConfig = JsonConvert.DeserializeObject<SteamRegistryConfig>(VdfFileReader.ToJson(File.ReadAllText("Resources/registry.vdf")));

            Assert.AreEqual("/home/whoever/.steam/debian-installation/steamapps\\sourcemods", steamConfig.HKCU.Software.Valve.Steam["SourceModInstallPath"]);
        }
    }
}
