using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDItemSearch.Utils.CharacterFiles
{
    public interface ICharacterFile
    {
        void Read(Stream f);

    }
}
