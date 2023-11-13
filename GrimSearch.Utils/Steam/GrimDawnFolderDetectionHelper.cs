using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Linq;

namespace GrimSearch.Utils.Steam
{
    public class GrimDawnWindowsFolderDetectionHelper
    {
        public string DetectGrimDawnDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                return DetectGrimDawnDirectoryOnWindows();
            }
            else if (OperatingSystem.IsLinux())
            {
                return DetectGrimDawnDirectoryOnLinux();
            }
            throw new NotSupportedException("Only Windows and Linux is supported for folder detection as of now. Please specify folders manually.");
        }

        public string DetectGrimDawnSavesDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                return DetectGrimDawnSavesDirectoryOnWindows();
            }
            else if (OperatingSystem.IsLinux())
            {
                return DetectGrimDawnSavesDirectoryOnLinux();
            }
            throw new NotSupportedException("Only Windows and Linux is supported for folder detection as of now. Please specify folders manually.");
        }

        /*
        Detecting GD folder on linux:
        - read ~/.steam/registry.vdf to get steam install folder from SourceModInstallPath, e.g. ~/.steam/debian-installation/steamapps/sourcemods (so steam folder is ~/.steam/debian-installation)
        - get libraryfolders.vdf from steamapps folder (e.g. ~/.steam/debian-installation/steamapps/libraryfolders.vdf) and find the library for the game id (219990)
        */
        [SupportedOSPlatform("linux")]
        private string DetectGrimDawnDirectoryOnLinux()
        {
            var registryVdfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "registry.vdf");

            if (!File.Exists(registryVdfPath))
            {
                throw new NotSupportedException("~/.steam/registry.vdf was not found -- folder detection not supported for your installation. Please specify folders manually.");
            }

            // Step 1: Find steamapps directory from SourceModInstallPath in registry.vdf
            var config = JsonConvert.DeserializeObject<SteamRegistryConfig>(VdfFileReader.ToJson(File.ReadAllText(registryVdfPath)));
            var sourcemodsFolder = config.HKCU.Software.Valve.Steam["SourceModInstallPath"].ToString().Replace("\\", "/");

            // Step 2: Find the library directory that contains game ID 219990 (which is Grim Dawn)
            var libraryFoldersVdf = Path.GetFullPath(Path.Combine(sourcemodsFolder, "..", "libraryfolders.vdf"));
            var libraryFoldersVdfJson = VdfFileReader.ToJson(File.ReadAllText(libraryFoldersVdf));
            var libraryFoldersParsed = JsonConvert.DeserializeObject<Dictionary<string, SteamLibraryFolderElement>>(libraryFoldersVdfJson);

            foreach (var entry in libraryFoldersParsed)
            {
                if (entry.Value.Apps.ContainsKey("219990"))
                {
                    var gdDir = Path.Combine(entry.Value.Path, "steamapps", "common", "Grim Dawn");
                    if (File.Exists(System.IO.Path.Combine(gdDir, "ArchiveTool.exe")))
                    {
                        return gdDir;
                    }
                }
            }

            throw new Exception("The Grim Dawn directory was not found in any of the library folders that were searched. Please specify folders manually.");
        }


        /*
        Detecting GD saves folder on linux:
        - for saves: find userid by checking list of folders in ~/.steam/steam/userdata
        - save folder would be: /.steam/steam/userdata/<userid>/219990/remote/save
        */
        [SupportedOSPlatform("linux")]
        private string DetectGrimDawnSavesDirectoryOnLinux()
        {
            var userdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam", "userdata");

            if (!Directory.Exists(userdataPath))
            {
                throw new NotSupportedException("~/.steam/steam/useradata was not found -- folder detection not supported for your installation. Please specify folders manually.");
            }

            // Step 1: Find steamapps directory from SourceModInstallPath in registry.vdf
            var userIds = Directory.EnumerateDirectories(userdataPath);
            if (userIds.Count() == 0)
            {
                throw new Exception("No user folder found in ~/.steam/steam/useradata.");
            }
            else if (userIds.Count() > 1)
            {
                throw new Exception("More than one user folder found in ~/.steam/steam/useradata. That most likely means more than one Steam account is used for this installation. Currently, folder detection is not supported for this scenario on Linux.");
            }
            var userIdFolder = userIds.First();

            var savesFolder = Path.Combine(userIdFolder, "219990", "remote", "save");
            if (!Path.Exists(savesFolder))
            {
                throw new Exception("Saves folder not found at " + savesFolder);
            }
            return savesFolder;
        }

        [SupportedOSPlatform("windows")]
        private string DetectGrimDawnDirectoryOnWindows()
        {
            string steamPath = GetRegistryValue<string>("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            int activeUser = GetRegistryValue<int>("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser");

            if (!Directory.Exists(steamPath))
                throw new InvalidOperationException("Steam path was not found. Is it installed?");

            string gdDir = GetInstallLocationOnWindows(steamPath);

            if (!File.Exists(System.IO.Path.Combine(gdDir, "ArchiveTool.exe")))
            {
                throw new Exception("The Grim Dawn directory was not found in the default install location for Steam games. Please specify this manually.");
            }

            return gdDir;
        }

        [SupportedOSPlatform("windows")]
        private string DetectGrimDawnSavesDirectoryOnWindows()
        {
            string steamPath = GetRegistryValue<string>("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            int activeUser = GetRegistryValue<int>("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser");

            if (!Directory.Exists(steamPath))
                throw new InvalidOperationException("Steam path was not found. Is it installed?");

            if (activeUser == 0)
                throw new InvalidOperationException("Steam is not running, or you are not logged in.");

            string savesDir = System.IO.Path.Combine(steamPath, "userdata", activeUser.ToString(), "219990", "remote", "save").Replace('/', '\\');

            if (!Directory.Exists(System.IO.Path.Combine(savesDir, "main")))
            {
                throw new Exception("Grim Dawn saves directory was not found at " + savesDir + ". Please specify this manually.");
            }
            return savesDir;
        }


        [SupportedOSPlatform("windows")]
        private string[] GetAllPossibleInstallLocationsOnWindows(string steamPath)
        {
            List<string> locations = new List<string>();
            locations.Add(Path.Combine(steamPath, "SteamApps", "common", "Grim Dawn").Replace('/', '\\'));
            locations.AddRange(GetInstallLocationsFromSteamConfigOnWindows(steamPath));


            return locations.ToArray();
        }

        [SupportedOSPlatform("windows")]
        private string[] GetInstallLocationsFromSteamConfigOnWindows(string steamPath)
        {
            var configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (!File.Exists(configPath))
                return new string[0];
            var configContent = File.ReadAllText(configPath);

            var configJson = VdfFileReader.ToJson(configContent);

            var deserialized = JsonConvert.DeserializeObject<SteamConfig>(configJson);

            var steamConfigInstallKeys = deserialized.Software.Valve.Steam.Keys.Where(x => x.StartsWith("BaseInstallFolder_"));

            List<string> results = new List<string>();

            foreach (var k in steamConfigInstallKeys)
            {
                var val = deserialized.Software.Valve.Steam[k] as string;
                if (string.IsNullOrEmpty(val))
                    continue;
                var fullGDPath = Path.Combine(val, "SteamApps", "common", "Grim Dawn").Replace('/', '\\');
                results.Add(fullGDPath);
            }

            return results.ToArray();
        }

        [SupportedOSPlatform("windows")]
        private string GetInstallLocationOnWindows(string steamPath)
        {
            var allLocations = GetAllPossibleInstallLocationsOnWindows(steamPath);
            foreach (var l in allLocations)
            {
                if (File.Exists(System.IO.Path.Combine(l, "ArchiveTool.exe")))
                    return l;
            }

            return null;
        }

        [SupportedOSPlatform("windows")]
        private T GetRegistryValue<T>(string path, string valueName)
        {
            var value = Registry.GetValue(path, valueName, null);

            if (value == null)
                return default(T);

            return (T)value;
        }
    }
}