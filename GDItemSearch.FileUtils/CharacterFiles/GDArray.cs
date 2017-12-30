using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.CharacterFiles
{
    public static class GDArray<T> where T:Readable, new()
    {
        public static List<T> Read(GDFileReader file)
        {
            List<T> entries = new List<T>();
            UInt32 n = file.ReadInt();

            for (var i = 0; i < n; i++)
            {
                var entry = new T();
                entry.Read(file);
                entries.Add(entry);
            }

            return entries;
        }
    }
}
