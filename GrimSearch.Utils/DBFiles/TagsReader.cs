using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.DBFiles
{
    public static class TagsReader
    {
        public static Dictionary<string, string> ReadAllTags(string path)
        {
            var content = File.ReadAllLines(path);
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var c in content)
            {
                if (string.IsNullOrWhiteSpace(c) || c.Trim().StartsWith("#") || !c.Contains('='))
                    continue;

                var splitLine = c.Split(new char[] { '=' }, 2);
                result[splitLine[0]] = splitLine[1];
            }

            return result;
        }
    }
}
