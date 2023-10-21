using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GrimSearch.Common;

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> objectsToAdd)
    {
        foreach (var o in objectsToAdd)
        {
            collection.Add(o);
        }
    }
}