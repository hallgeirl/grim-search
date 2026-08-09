using System.IO;
using System.Linq;
using GrimSearch.Utils.Steam;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrimSearch.Tests.Utils.Steam
{
    [TestClass]
    public class GrimDawnFolderDetectionHelperTests
    {
        [TestMethod]
        public void FindGrimDawnInstallLocationReturnsDirectoryContainingArchiveTool()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var invalidLocation = Directory.CreateDirectory(Path.Combine(root.FullName, "invalid")).FullName;
                var gogLocation = Directory.CreateDirectory(Path.Combine(root.FullName, "GOG Grim Dawn")).FullName;
                File.WriteAllText(Path.Combine(gogLocation, "ArchiveTool.exe"), string.Empty);

                var result = GrimDawnWindowsFolderDetectionHelper.FindGrimDawnInstallLocation(
                    new[] { invalidLocation, gogLocation });

                Assert.AreEqual(gogLocation, result);
            }
            finally
            {
                root.Delete(true);
            }
        }

        [TestMethod]
        public void FindGrimDawnSavesLocationReturnsDirectoryContainingMainFolder()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var invalidLocation = Directory.CreateDirectory(Path.Combine(root.FullName, "invalid")).FullName;
                var gogSaves = Directory.CreateDirectory(Path.Combine(root.FullName, "My Games", "Grim Dawn", "save")).FullName;
                Directory.CreateDirectory(Path.Combine(gogSaves, "main"));

                var result = GrimDawnWindowsFolderDetectionHelper.FindGrimDawnSavesLocation(
                    new[] { invalidLocation, gogSaves });

                Assert.AreEqual(gogSaves, result);
            }
            finally
            {
                root.Delete(true);
            }
        }

        [TestMethod]
        public void GetSteamSaveLocationsFindsSaveWithoutAnActiveSteamUser()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var olderSave = CreateSteamSave(root.FullName, "100");
                var newerSave = CreateSteamSave(root.FullName, "200");
                Directory.SetLastWriteTimeUtc(Path.Combine(olderSave, "main"), new System.DateTime(2020, 1, 1));
                Directory.SetLastWriteTimeUtc(Path.Combine(newerSave, "main"), new System.DateTime(2021, 1, 1));

                var results = GrimDawnWindowsFolderDetectionHelper.GetSteamSaveLocations(new[] { root.FullName }).ToArray();

                CollectionAssert.AreEqual(new[] { newerSave, olderSave }, results);
            }
            finally
            {
                root.Delete(true);
            }
        }

        [TestMethod]
        public void GetSteamSaveLocationsPrefersActiveUserWhenSteamIsRunning()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var preferredSave = CreateSteamSave(root.FullName, "100");
                var newerSave = CreateSteamSave(root.FullName, "200");
                Directory.SetLastWriteTimeUtc(Path.Combine(preferredSave, "main"), new System.DateTime(2020, 1, 1));
                Directory.SetLastWriteTimeUtc(Path.Combine(newerSave, "main"), new System.DateTime(2021, 1, 1));

                var results = GrimDawnWindowsFolderDetectionHelper
                    .GetSteamSaveLocations(new[] { root.FullName }, "100")
                    .ToArray();

                Assert.AreEqual(preferredSave, results[0]);
            }
            finally
            {
                root.Delete(true);
            }
        }

        [TestMethod]
        public void GetSteamInstallLocationsReadsModernLibraryFoldersFile()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var libraryRoot = Directory.CreateDirectory(Path.Combine(root.FullName, "library")).FullName;
                var steamApps = Directory.CreateDirectory(Path.Combine(root.FullName, "steamapps")).FullName;
                File.WriteAllText(Path.Combine(steamApps, "libraryfolders.vdf"),
                    $"\"libraryfolders\"\n{{\n\t\"0\"\n\t{{\n\t\t\"path\" \"{libraryRoot}\"\n\t\t\"apps\"\n\t\t{{\n\t\t\t\"219990\" \"1\"\n\t\t}}\n\t}}\n}}");

                var results = GrimDawnWindowsFolderDetectionHelper.GetSteamInstallLocations(root.FullName).ToArray();

                CollectionAssert.Contains(results, Path.Combine(libraryRoot, "steamapps", "common", "Grim Dawn"));
            }
            finally
            {
                root.Delete(true);
            }
        }

        [TestMethod]
        public void GetLinuxSteamRootsFindsStandardAndFlatpakInstallations()
        {
            var root = Directory.CreateTempSubdirectory();
            try
            {
                var standardRoot = Directory.CreateDirectory(Path.Combine(root.FullName, ".local", "share", "Steam")).FullName;
                var flatpakRoot = Directory.CreateDirectory(Path.Combine(
                    root.FullName, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam")).FullName;

                var results = GrimDawnWindowsFolderDetectionHelper.GetLinuxSteamRoots(root.FullName).ToArray();

                CollectionAssert.Contains(results, standardRoot);
                CollectionAssert.Contains(results, flatpakRoot);
            }
            finally
            {
                root.Delete(true);
            }
        }

        private static string CreateSteamSave(string steamRoot, string userId)
        {
            var savePath = Path.Combine(steamRoot, "userdata", userId, "219990", "remote", "save");
            Directory.CreateDirectory(Path.Combine(savePath, "main"));
            return savePath;
        }
    }
}
