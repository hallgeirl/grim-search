using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GDItemSearch.Common
{
    //Reader for the Valve config file format
    public static class VdfFileReader
    {
        public static string ToJson(string vdfContent)
        {
            var keyRegex = "\"[^\"]+\"";
            var valueRegex = "\"[^\"]*\"";
            var lines = vdfContent.Split('\n').ToList();

            lines.RemoveAt(0);

            var trimmedVdf = string.Join("\n", lines);

            //From:
            //"key" "value" 
            //to:
            //"key": "value"
            var json = Regex.Replace(trimmedVdf, "(" + keyRegex + ")\\s+(" + valueRegex + ")", "$1: $2", RegexOptions.Multiline);

            //From:
            //"key" { 
            //to:
            //"key": {
            json = Regex.Replace(json, "(" + keyRegex + ")\\s+{", "$1: {", RegexOptions.Multiline);

            //From:
            //"key": "value"
            //"key2": "value2"
            //to:
            //"key": "value",
            //"key2": "value2"
            //(Need to run twice to catch all)
            json = Regex.Replace(json, "(" + keyRegex + "):\\s+(" + valueRegex + ")\\s+(" + keyRegex + "):", "$1: $2,\r\n$3:", RegexOptions.Multiline);
            json = Regex.Replace(json, "(" + keyRegex + "):\\s+(" + valueRegex + ")\\s+(" + keyRegex + "):", "$1: $2,\r\n$3:", RegexOptions.Multiline);

            //From:
            //}
            //"key2": "value2"
            //to:
            //},
            //"key2": "value2"
            //(Need to run twice to catch all)
            json = Regex.Replace(json, "}\\s+(" + keyRegex + "):", "},\r\n$1:", RegexOptions.Multiline);
            json = Regex.Replace(json, "}\\s+(" + keyRegex + "):", "},\r\n$1:", RegexOptions.Multiline);

            return json;
        }
    }
}
