using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils
{
    public class MD5Store
    {
        private MD5Store()
        {
        }

        private Dictionary<string, string> _hashes = new Dictionary<string, string>();

        static MD5Store _instance = new MD5Store();
        public static MD5Store Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
                return;

            _hashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        }

        public void Save(string path)
        {
            var hashesJson = JsonConvert.SerializeObject(_hashes);
            File.WriteAllText(path, hashesJson);
        }

        public void SetHash(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    _hashes[path] = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }
    }
}
