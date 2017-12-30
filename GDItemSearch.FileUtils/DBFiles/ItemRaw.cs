using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.FileUtils.DBFiles
{
    public class ItemRaw
    {
        private string _name;
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(ItemStyle))
                    return ItemStyle + " " + _name;

                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Type { get; set; }
        public string ItemStyle { get; set; }
        public int LevelRequirement { get; set; }
        public string ItemClassification { get; set; }

        public Dictionary<string, float> NumericalParametersRaw = new Dictionary<string, float>();

        private static string[] ignoredProperties =
        {
            "templateName",
            "actorHeight",
            "actorRadius",
            "allowTransparency",
            "armorFemaleMesh",
            "armorMaleMesh",
            "attributeScalePercent",
            "baseTexture",
            "bitmap",
            "bumpTexture"
        };

        public void Read(string dbrPath)
        {
            var lines = File.ReadAllLines(dbrPath);

            foreach (var l in lines)
            {
                var splitLine = l.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (ShouldIgnore(splitLine))
                    continue;

                if (ParseSpecials(splitLine))
                    continue;

                ParseNumerical(splitLine);
            }
        }

        private bool ShouldIgnore(string[] line)
        {
            if (line.Length == 0)
                return true;

            return ignoredProperties.Contains(line[0]);
        }

        private bool ParseSpecials(string[] line)
        {
            switch (line[0])
            {
                case "Class":
                    Type = line[1];
                    return true;

                case "FileDescription":
                    Name = line[1];
                    return true;

                case "itemStyleTag":
                    switch (line[1])
                    {
                        case "tagStyleUniqueTier2":
                            ItemStyle = "Empowered";
                            break;

                        case "tagStyleUniqueTier3":
                            ItemStyle = "Mythical";
                            break;

                        case "tagStyleFactionTier2":
                            ItemStyle = "Elite";
                            break;
                    }
                    return true;
                case "levelRequirement":
                    LevelRequirement = int.Parse(line[1]);
                    return true;
                case "itemClassification":
                    ItemClassification = line[1];
                    return true;
            }

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
    }
}
