using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.ViewModels
{
    public class SteamConfigSoftwareElement
    {
        public SteamConfigValveElement Valve { get; set; }
    }

    public class SteamConfigValveElement
    {
        public Dictionary<string, object> Steam { get; set; }
    }

    public class SteamConfig
    {
        public SteamConfigSoftwareElement Software { get; set; }
    }
}
