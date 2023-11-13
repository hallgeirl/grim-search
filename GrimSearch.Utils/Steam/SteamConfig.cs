using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils.Steam
{
    public class SteamConfigSoftwareElement
    {
        public SteamConfigValveElement Valve { get; set; }
    }

    public class SteamConfigValveElement
    {
        public Dictionary<string, object> Steam { get; set; }
    }

    // used for reading registry.vdf
    public class SteamConfigHKCUElement
    {
        public SteamConfigSoftwareElement Software { get; set; }
    }

    public class SteamConfig
    {
        public SteamConfigSoftwareElement Software { get; set; }
    }

    public class SteamRegistryConfig
    {
        public SteamConfigHKCUElement HKCU { get; set; }
    }
}
