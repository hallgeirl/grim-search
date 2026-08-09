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
        private const string SteamGrimDawnAppId = "219990";
        private const string GogGrimDawnGameId = "1449651388";

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

        [SupportedOSPlatform("linux")]
        private string DetectGrimDawnDirectoryOnLinux()
        {
            var steamRoots = GetLinuxSteamRoots(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var gdDir = FindGrimDawnInstallLocation(steamRoots.SelectMany(GetSteamInstallLocations));

            if (gdDir != null)
                return gdDir;

            throw new Exception("The Grim Dawn directory was not found in any of the library folders that were searched. Please specify folders manually.");
        }

        [SupportedOSPlatform("linux")]
        private string DetectGrimDawnSavesDirectoryOnLinux()
        {
            var steamRoots = GetLinuxSteamRoots(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var savesFolder = FindGrimDawnSavesLocation(GetSteamSaveLocations(steamRoots));

            if (savesFolder != null)
                return savesFolder;

            throw new Exception("The Grim Dawn saves directory was not found in any Steam user folder that was searched. Please specify this manually.");
        }

        [SupportedOSPlatform("windows")]
        private string DetectGrimDawnDirectoryOnWindows()
        {
            string steamPath = GetRegistryValue<string>("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            var installLocations = new List<string>();

            if (Directory.Exists(steamPath))
            {
                installLocations.AddRange(GetSteamInstallLocations(steamPath));
                installLocations.AddRange(GetInstallLocationsFromSteamConfigOnWindows(steamPath));
            }

            installLocations.AddRange(GetGogInstallLocationsOnWindows());

            var gdDir = FindGrimDawnInstallLocation(installLocations);
            if (gdDir != null)
                return gdDir;

            throw new Exception("The Grim Dawn directory was not found in the Steam or GOG install locations that were searched. Please specify this manually.");
        }

        [SupportedOSPlatform("windows")]
        private string DetectGrimDawnSavesDirectoryOnWindows()
        {
            string steamPath = GetRegistryValue<string>("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            int activeUser = GetRegistryValue<int>("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser");
            var saveLocations = new List<string>();

            if (Directory.Exists(steamPath))
                saveLocations.AddRange(GetSteamSaveLocations(new[] { steamPath }, activeUser == 0 ? null : activeUser.ToString()));

            // GOG uses Grim Dawn's local save location rather than Steam Cloud's userdata folder.
            saveLocations.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Grim Dawn", "save"));

            var savesDir = FindGrimDawnSavesLocation(saveLocations);
            if (savesDir != null)
                return savesDir;

            throw new Exception("The Grim Dawn saves directory was not found in the Steam or GOG save locations that were searched. Please specify this manually.");
        }

        [SupportedOSPlatform("windows")]
        private IEnumerable<string> GetGogInstallLocationsOnWindows()
        {
            var registryPaths = new[]
            {
                $"HKEY_LOCAL_MACHINE\\SOFTWARE\\GOG.com\\Games\\{GogGrimDawnGameId}",
                $"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\GOG.com\\Games\\{GogGrimDawnGameId}"
            };

            return registryPaths
                .Select(path => GetRegistryValue<string>(path, "path"))
                .Where(path => !string.IsNullOrWhiteSpace(path));
        }

        internal static string FindGrimDawnInstallLocation(IEnumerable<string> locations)
        {
            return locations.FirstOrDefault(location =>
                !string.IsNullOrWhiteSpace(location) && File.Exists(Path.Combine(location, "ArchiveTool.exe")));
        }

        internal static string FindGrimDawnSavesLocation(IEnumerable<string> locations)
        {
            return locations.FirstOrDefault(location =>
                !string.IsNullOrWhiteSpace(location) && Directory.Exists(Path.Combine(location, "main")));
        }

        internal static IEnumerable<string> GetSteamInstallLocations(string steamRoot)
        {
            var locations = new List<string>
            {
                Path.Combine(steamRoot, "steamapps", "common", "Grim Dawn")
            };

            var libraryFoldersPath = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFoldersPath))
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<Dictionary<string, SteamLibraryFolderElement>>(
                        VdfFileReader.ToJson(File.ReadAllText(libraryFoldersPath)));

                    locations.AddRange((parsed ?? new Dictionary<string, SteamLibraryFolderElement>())
                        .Values
                        .Where(library => library != null && !string.IsNullOrWhiteSpace(library.Path))
                        .Select(library => Path.Combine(library.Path, "steamapps", "common", "Grim Dawn")));
                }
                catch (JsonException)
                {
                    // A damaged Steam config should not prevent checking other known locations.
                }
                catch (IOException)
                {
                    // Steam may be replacing the config while detection is in progress.
                }
                catch (UnauthorizedAccessException)
                {
                    // Continue with the default library if the config cannot be read.
                }
            }

            return locations.Distinct(GetPathComparer());
        }

        internal static IEnumerable<string> GetSteamSaveLocations(IEnumerable<string> steamRoots, string preferredUserId = null)
        {
            var locations = new List<(string UserId, string SavePath, DateTime LastWriteTime)>();

            foreach (var steamRoot in steamRoots.Distinct(GetPathComparer()))
            {
                var userdataPath = Path.Combine(steamRoot, "userdata");
                if (!Directory.Exists(userdataPath))
                    continue;

                try
                {
                    foreach (var userDirectory in Directory.EnumerateDirectories(userdataPath))
                    {
                        var savePath = Path.Combine(userDirectory, SteamGrimDawnAppId, "remote", "save");
                        var mainPath = Path.Combine(savePath, "main");
                        if (Directory.Exists(mainPath))
                        {
                            locations.Add((Path.GetFileName(userDirectory), savePath, Directory.GetLastWriteTimeUtc(mainPath)));
                        }
                    }
                }
                catch (IOException)
                {
                    // Continue with other Steam roots if one userdata folder is unavailable.
                }
                catch (UnauthorizedAccessException)
                {
                    // Continue with other Steam roots if one userdata folder is unavailable.
                }
            }

            return locations
                .OrderByDescending(location => location.UserId == preferredUserId)
                .ThenByDescending(location => location.LastWriteTime)
                .Select(location => location.SavePath);
        }

        internal static IEnumerable<string> GetLinuxSteamRoots(string userProfile)
        {
            var roots = new List<string>
            {
                Path.Combine(userProfile, ".steam", "steam"),
                Path.Combine(userProfile, ".steam", "root"),
                Path.Combine(userProfile, ".steam", "debian-installation"),
                Path.Combine(userProfile, ".local", "share", "Steam"),
                Path.Combine(userProfile, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
                Path.Combine(userProfile, "snap", "steam", "common", ".local", "share", "Steam")
            };

            var registryVdfPath = Path.Combine(userProfile, ".steam", "registry.vdf");
            if (File.Exists(registryVdfPath))
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<SteamRegistryConfig>(
                        VdfFileReader.ToJson(File.ReadAllText(registryVdfPath)));
                    var steamConfig = config?.HKCU?.Software?.Valve?.Steam;

                    if (steamConfig != null && steamConfig.TryGetValue("SourceModInstallPath", out var sourceModsValue))
                    {
                        var sourceModsPath = sourceModsValue?.ToString()?.Replace("\\", "/");
                        if (!string.IsNullOrWhiteSpace(sourceModsPath))
                            roots.Add(Path.GetFullPath(Path.Combine(sourceModsPath, "..", "..")));
                    }
                }
                catch (JsonException)
                {
                    // Fall back to standard Steam locations.
                }
                catch (IOException)
                {
                    // Fall back to standard Steam locations.
                }
                catch (UnauthorizedAccessException)
                {
                    // Fall back to standard Steam locations.
                }
            }

            return roots.Where(Directory.Exists).Distinct(GetPathComparer());
        }

        private static StringComparer GetPathComparer()
        {
            return OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }

        [SupportedOSPlatform("windows")]
        private string[] GetInstallLocationsFromSteamConfigOnWindows(string steamPath)
        {
            var configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (!File.Exists(configPath))
                return new string[0];
            try
            {
                var configJson = VdfFileReader.ToJson(File.ReadAllText(configPath));
                var steamConfig = JsonConvert.DeserializeObject<SteamConfig>(configJson)?.Software?.Valve?.Steam;
                if (steamConfig == null)
                    return Array.Empty<string>();

                return steamConfig
                    .Where(entry => entry.Key.StartsWith("BaseInstallFolder_") && entry.Value is string)
                    .SelectMany(entry =>
                    {
                        var libraryPath = (string)entry.Value;
                        return new[]
                        {
                            Path.Combine(libraryPath, "SteamApps", "common", "Grim Dawn"),
                            Path.Combine(libraryPath, "common", "Grim Dawn")
                        };
                    })
                    .Distinct(GetPathComparer())
                    .ToArray();
            }
            catch (JsonException)
            {
                return Array.Empty<string>();
            }
            catch (IOException)
            {
                return Array.Empty<string>();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
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
