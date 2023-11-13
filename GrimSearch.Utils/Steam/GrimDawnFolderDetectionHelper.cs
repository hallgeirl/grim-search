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

            }
            throw new NotSupportedException("Only Windows and Linux is supported for folder detection as of now. Please specify folders manually.");
        }




        [SupportedOSPlatform("linux")]
        private string DetectGrimDawnDirectoryOnLinux()
        {
            return null;
            /*var registryVdfPath = "~/.steam/registry.vdf";
            if (!File.Exists(registryVdfPath))
            {
                throw new NotSupportedException("~/.steam/registry.vdf was not found -- folder detection not supported for your installation. Please specify folders manually.");
            }

            var config = JsonConvert.DeserializeObject(VdfFileReader.ToJson(File.ReadAllText(registryVdfPath)));

            if (!Directory.Exists(steamPath))
                throw new InvalidOperationException("Steam path was not found. Is it installed?");

            string gdDir = GetInstallLocationOnWindows(steamPath);

            if (!File.Exists(System.IO.Path.Combine(gdDir, "ArchiveTool.exe")))
            {
                throw new Exception("The Grim Dawn directory was not found in the default install location for Steam games. Please specify this manually.");
            }

            return gdDir;*/
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