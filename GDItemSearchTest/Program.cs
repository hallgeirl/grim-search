using GDItemSearch.FileUtils;
using GDItemSearch.FileUtils.CharacterFiles;
using GDItemSearch.FileUtils.DBFiles;
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
//                var file = new CharacterFile();
  //              file.Read(s);

                Index index = new Index();
                index.Build();

                var res = index.Find("ultos", new IndexFilter());
                var res2 = index.Find("ultos", new IndexFilter() { MaxLevel = 80 });


                //var itemCache = new ItemCache();
                //itemCache.LoadAllItems();

                //var path = new ArzExtractor().Extract(Settings.GrimDawnDirectory + "\\database\\database.arz");
                //var path = new ArzExtractor().Extract(Settings.GrimDawnDirectory + "\\resources\\UI.arc");
                //var path = new ArzExtractor().Extract(Settings.GrimDawnDirectory + "\\database\\templates.arc");
                //var path = new ArzExtractor().Extract(Settings.GrimDawnDirectory + "\\gdx1\\database\\GDX1.arz");



            }
        }
    }
}
