using GDItemSearchUtilFileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var s = File.OpenRead("player.gdc"))
            {
                var file = new CharacterFile();


                file.Read(s);
            }
        }
    }
}
