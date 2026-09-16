using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrimSearch.Utils.DBFiles
{
    public class StringsCache
    {
        private sealed class StringsCacheContainer
        {
            public string Language { get; set; }
            public Dictionary<string, string> ActiveStrings { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> EnglishStrings { get; set; } = new Dictionary<string, string>();
        }

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private Dictionary<string, string> _activeStrings = new Dictionary<string, string>();
        private Dictionary<string, string> _englishStrings = new Dictionary<string, string>();
        private bool _initialized;

        public string CacheFilename { get; set; }
        public bool IsDirty { get; set; } = true;
        public string Language { get; set; } = "EN";

        private StringsCache()
        {
            CacheFilename = ConfigFileHelper.GetConfigFile("TagsCache.json");
        }

        public string GetString(string tagName)
        {
            EnsureInitialized();
            if (_activeStrings.TryGetValue(tagName, out var value) && !string.IsNullOrEmpty(value))
                return value;
            return GetEnglishString(tagName);
        }

        public string GetEnglishString(string tagName)
        {
            EnsureInitialized();
            return _englishStrings.TryGetValue(tagName, out var value) ? value : null;
        }

        public void LoadAllStrings(string grimDawnDirectory)
        {
            var language = NormalizeLanguage(Language);
            if (TryLoadCache(language))
            {
                _initialized = true;
                IsDirty = false;
                return;
            }

            _englishStrings = ReadTagsFromFiles(grimDawnDirectory, "EN");
            _activeStrings = language == "EN"
                ? new Dictionary<string, string>(_englishStrings)
                : ReadTagsFromFiles(grimDawnDirectory, language);

            var cache = new StringsCacheContainer
            {
                Language = language,
                ActiveStrings = _activeStrings,
                EnglishStrings = _englishStrings
            };
            File.WriteAllText(CacheFilename, JsonConvert.SerializeObject(cache));

            Language = language;
            _initialized = true;
            IsDirty = false;
        }

        public static IReadOnlyList<string> GetAvailableLanguages(string grimDawnDirectory)
        {
            if (string.IsNullOrWhiteSpace(grimDawnDirectory))
                return new[] { "EN" };

            var resourcesDirectory = Path.Combine(grimDawnDirectory, "resources");
            if (!Directory.Exists(resourcesDirectory))
                return new[] { "EN" };

            var languages = Directory.EnumerateFiles(resourcesDirectory, "Text_*.arc", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetFileNameWithoutExtension(path).Substring("Text_".Length))
                .Select(NormalizeLanguage)
                .Where(language => !string.IsNullOrWhiteSpace(language))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            languages.Add("EN");

            return languages.OrderBy(language => language == "EN" ? "" : language).ToArray();
        }

        public void ClearCache()
        {
            IsDirty = true;
            _initialized = false;
            if (File.Exists(CacheFilename))
                File.Delete(CacheFilename);
        }

        internal void SetStringsForTesting(Dictionary<string, string> activeStrings, Dictionary<string, string> englishStrings, string language)
        {
            _activeStrings = new Dictionary<string, string>(activeStrings);
            _englishStrings = new Dictionary<string, string>(englishStrings);
            Language = NormalizeLanguage(language);
            _initialized = true;
            IsDirty = false;
        }

        private bool TryLoadCache(string language)
        {
            if (!File.Exists(CacheFilename))
                return false;

            var root = JObject.Parse(File.ReadAllText(CacheFilename));

            // Compatibility with the original English-only Dictionary<string, string> cache.
            if (root["Language"] == null)
            {
                if (language != "EN")
                    return false;

                _englishStrings = root.ToObject<Dictionary<string, string>>();
                _activeStrings = new Dictionary<string, string>(_englishStrings);
                Language = "EN";
                return true;
            }

            var cache = root.ToObject<StringsCacheContainer>();
            if (!string.Equals(NormalizeLanguage(cache.Language), language, StringComparison.OrdinalIgnoreCase))
                return false;

            _activeStrings = cache.ActiveStrings ?? new Dictionary<string, string>();
            _englishStrings = cache.EnglishStrings ?? new Dictionary<string, string>();
            Language = language;
            return true;
        }

        private static Dictionary<string, string> ReadTagsFromFiles(string grimDawnDirectory, string language)
        {
            var result = new Dictionary<string, string>();
            foreach (var fullFilePath in GetTagArchives(grimDawnDirectory, language))
            {
                if (!File.Exists(fullFilePath))
                    continue;

                var tempArcFile = GetTagFileCopy(fullFilePath);
                var extractedPath = Path.Combine(Path.GetTempPath(), "GDArchiveTempPath", Path.GetFileNameWithoutExtension(fullFilePath) + "_" + Guid.NewGuid());
                try
                {
                    ArzExtractor.ExtractArc(tempArcFile, extractedPath);
                    foreach (var file in Directory.EnumerateFiles(extractedPath, "*.txt", SearchOption.AllDirectories))
                    {
                        foreach (var tag in TagsReader.ReadAllTags(file))
                            result[tag.Key] = tag.Value;
                    }
                    MD5Store.Instance.SetHash(fullFilePath);
                }
                finally
                {
                    if (Directory.Exists(extractedPath))
                        Directory.Delete(extractedPath, true);

                    var tempArcDirectory = Path.GetDirectoryName(tempArcFile);
                    if (Directory.Exists(tempArcDirectory))
                        Directory.Delete(tempArcDirectory, true);
                }
            }
            return result;
        }

        private static IEnumerable<string> GetTagArchives(string grimDawnDirectory, string language)
        {
            if (string.IsNullOrWhiteSpace(grimDawnDirectory))
                return Array.Empty<string>();

            var archiveName = $"Text_{language}.arc";
            if (language != "EN")
                return new[] { Path.Combine(grimDawnDirectory, "resources", archiveName) };

            return new[]
            {
                Path.Combine(grimDawnDirectory, "resources", archiveName),
                Path.Combine(grimDawnDirectory, "gdx1", "resources", archiveName),
                Path.Combine(grimDawnDirectory, "gdx2", "resources", archiveName),
                Path.Combine(grimDawnDirectory, "gdx3", "resources", archiveName)
            };
        }

        private static string GetTagFileCopy(string tagFilePath)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var targetFile = Path.Combine(tempDir, Path.GetFileName(tagFilePath));
            _logger.Info("Copying " + tagFilePath + " to " + targetFile);
            Directory.CreateDirectory(tempDir);
            File.Copy(tagFilePath, targetFile);
            return targetFile;
        }

        private static string NormalizeLanguage(string language) =>
            string.IsNullOrWhiteSpace(language) ? "EN" : language.Trim().ToUpperInvariant();

        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("StringsCache has not yet been initialized! Call LoadAllStrings FIRST before calling GetString.");
        }

        private static readonly StringsCache _instance = new StringsCache();
        public static StringsCache Instance => _instance;
    }
}
