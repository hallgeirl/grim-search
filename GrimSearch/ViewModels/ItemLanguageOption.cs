using System.Collections.Generic;

namespace GrimSearch.ViewModels
{
    public sealed class ItemLanguageOption
    {
        private static readonly IReadOnlyDictionary<string, string> LanguageNames =
            new Dictionary<string, string>
            {
                ["BG"] = "Bulgarian",
                ["CS"] = "Czech",
                ["DE"] = "German",
                ["EL"] = "Greek",
                ["EN"] = "English",
                ["ES"] = "Spanish",
                ["FR"] = "French",
                ["HU"] = "Hungarian",
                ["IT"] = "Italian",
                ["JA"] = "Japanese",
                ["KO"] = "Korean",
                ["NL"] = "Dutch",
                ["PL"] = "Polish",
                ["PT"] = "Portuguese",
                ["RU"] = "Russian",
                ["SK"] = "Slovak",
                ["TH"] = "Thai",
                ["TR"] = "Turkish",
                ["VI"] = "Vietnamese",
                ["ZH"] = "Chinese"
            };

        public ItemLanguageOption(string code)
        {
            Code = code.ToUpperInvariant();
            DisplayName = LanguageNames.TryGetValue(Code, out var name) ? name : Code;
        }

        public string Code { get; }
        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}
