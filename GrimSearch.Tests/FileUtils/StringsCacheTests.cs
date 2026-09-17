using GrimSearch.Utils.DBFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace GrimSearch.Tests.FileUtils
{
    [TestClass]
    public class StringsCacheTests
    {
        [TestMethod]
        public void UsesLocalizedValueAndFallsBackPerTagToEnglish()
        {
            StringsCache.Instance.SetStringsForTesting(
                new Dictionary<string, string> { ["localized"] = "Espada" },
                new Dictionary<string, string> { ["localized"] = "Sword", ["fallback"] = "Shield" },
                "ES");

            Assert.AreEqual("Espada", StringsCache.Instance.GetString("localized"));
            Assert.AreEqual("Shield", StringsCache.Instance.GetString("fallback"));
            Assert.AreEqual("Sword", StringsCache.Instance.GetEnglishString("localized"));
        }

        [TestMethod]
        public void DiscoversAllInstalledLanguageArchives()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var resources = Directory.CreateDirectory(Path.Combine(root, "resources")).FullName;
            try
            {
                File.WriteAllBytes(Path.Combine(resources, "Text_ZH.arc"), new byte[0]);
                File.WriteAllBytes(Path.Combine(resources, "text_ja.arc"), new byte[0]);
                File.WriteAllBytes(Path.Combine(resources, "TEXT_RU.ARC"), new byte[0]);

                CollectionAssert.AreEqual(
                    new[] { "EN", "JA", "RU", "ZH" },
                    new List<string>(StringsCache.GetAvailableLanguages(root)));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ArchiveFingerprintsDetectChangedAndAddedArchives()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var resources = Directory.CreateDirectory(Path.Combine(root, "resources")).FullName;
            try
            {
                var englishArchive = Path.Combine(resources, "Text_EN.arc");
                File.WriteAllBytes(englishArchive, new byte[] { 1 });
                var cached = StringsCache.GetArchiveFingerprints(root, "EN");

                Assert.IsTrue(StringsCache.ArchiveFingerprintsMatch(
                    cached, StringsCache.GetArchiveFingerprints(root, "EN")));

                File.WriteAllBytes(englishArchive, new byte[] { 1, 2 });
                Assert.IsFalse(StringsCache.ArchiveFingerprintsMatch(
                    cached, StringsCache.GetArchiveFingerprints(root, "EN")));

                File.WriteAllBytes(Path.Combine(resources, "text_de.ARC"), new byte[] { 3 });
                Assert.IsFalse(StringsCache.ArchiveFingerprintsMatch(
                    cached, StringsCache.GetArchiveFingerprints(root, "DE")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
