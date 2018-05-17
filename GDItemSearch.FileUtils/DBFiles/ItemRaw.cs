using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils.DBFiles
{
    public class ItemRaw
    {
        public Dictionary<string, float> NumericalParametersRaw = new Dictionary<string, float>();
        public Dictionary<string, string> StringParametersRaw = new Dictionary<string, string>();

        public void Read(string dbrPath)
        {
            var lines = File.ReadAllLines(dbrPath);

            foreach (var l in lines)
            {
                var splitLine = l.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (ShouldIgnore(splitLine))
                    continue;

                if (ParseNumerical(splitLine))
                    continue;

                ParseAny(splitLine);
            }
        }

        private bool ShouldIgnore(string[] line)
        {
            if (line.Length == 0)
                return true;

            return false;
        }

        private bool ParseNumerical(string[] line)
        {
            float res;
            var success = float.TryParse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture, out res);

            if (success && res != 0)
                NumericalParametersRaw[line[0]] = res;

            return success;
        }

        private bool ParseAny(string[] line)
        {
            if (line != null && line.Length >= 2)
            {
                StringParametersRaw[line[0]] = String.Join(",", line.Skip(1).ToArray());
                return true;
            }

            return false;
        }
    }
}
